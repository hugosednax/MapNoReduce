﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PADIMapInterfaces;
using System.Reflection;
using System.Runtime.Remoting;
using System.Timers;
using System.Diagnostics;
using System.Threading;
using System.Net.Sockets;
using System.IO;

namespace MapWorker
{
    public class Worker : MarshalByRefObject, IWorker
    {
        #region Class Variables
        static int id, port, inputSize, numSlices;
        static int indexesToComplete = 1;
        static int indexesCompleted = 0;
        static string ip, clientUrl;
        static string jtUrl = "";
        static string jtSubUrl = "";
        static string backingUrl = "";
        static string backerUrl = "";
        System.Timers.Timer aTimer = new System.Timers.Timer();
        System.Timers.Timer slowTimer;
        System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
        static int currentTerm = 0;
        static int myTerm = 0;
        static int backingIndexes = 0;
        static byte[] dll = null;
        static string mapperName = null;
        static Assembly assembly = null;
        static JobMap backingJob;
        static bool cancelJob = false;
        
        #region MachineFlags
        static bool communicable        = true;
        static bool JTcommunicable      = true;
        static bool notYetSent = true;
        static bool available = true;
        static bool hasAssignedJobs = false;
        static bool isSlowed = false;
        static bool computing = false;
        static bool forceJobSub = false;
        static bool isOnTimeEvent = false;
		public enum JTStatus {ISJT, ISJTSUB, NONE};
		JTStatus currJTStatus = JTStatus.NONE;
        #endregion
        #region Structures
        static JobMap currentJob = new JobMap("",-1,-1);
        static Queue<KeyValuePair<int, int>> onHoldJobs = new Queue<KeyValuePair<int, int>>();
        static List<KeyValuePair<string, string>> subHalfOutput;
        static HashSet<string> workersURL = new HashSet<string>();
        static Dictionary<string, IWorker> mapProxies = new Dictionary<string, IWorker>();
        static HashSet<string> failedWorkers = new HashSet<string>();
        static Dictionary<string, ProgressReport> progresses = new Dictionary<string, ProgressReport>();
        #endregion
        #region Delegates
        public delegate void RemoteGiveJobDelegate(int splitBegin, int splitEnd, string clientUrl, string callerUrl, int term, string backerUrl);
        public delegate void RemoteCliAsyncDelegate(IList<KeyValuePair<string,string>> list, int begin, int end, string jobTrackerUrl);
        public delegate void jtSubOnHoldJobsDelegate(Queue<KeyValuePair<int, int>> onHoldJobs);
        public delegate void notJTDelegate();
        public delegate void newJTDelegate(string callerUrl, int term);
        #endregion
        #endregion

        #region Constructors
        public Worker(int _id, string _ip, int _port)
        {
            id = _id;
            ip = _ip;
            port = _port;
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Interval = 1500;
            aTimer.Enabled = true;
        }

        public Worker(int _id, string _ip, int _port, string entrypointURL)
        {
            id = _id;
            ip = _ip;
            port = _port;
            workerNetConnect(entrypointURL);
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Interval = 1500;
            aTimer.Enabled = true;
        }

        public override object InitializeLifetimeService()
        {

            return null;

        }
        #endregion

        #region Getters
        public static string getUrl() { return "tcp://" + ip + ":" + port + "/W"; }

        private static IWorker getRemoteWorker(string objUrl)
        {
            if (mapProxies.ContainsKey(objUrl))
                return mapProxies[objUrl];
            else {
                IWorker newWorker = (IWorker)Activator.GetObject(typeof(IWorker),objUrl);
                mapProxies.Add(objUrl, newWorker);
                return newWorker;
            }
        }
        #endregion

        #region Private Class Methods
        private bool CheckTerm(string callerUrl, int term)
        {
            bool isCallerJT = true;
            if (term < currentTerm)
            {
                Console.WriteLine("[Invalid Call] Node that is not JT (" + callerUrl + ") tried to invoke me");
                isCallerJT = false;
                try
                {
                    IWorker wk = getRemoteWorker(callerUrl);
                    notJTDelegate del = new notJTDelegate(wk.NotifyNotJT);
                    del.BeginInvoke(null, null);
                    wk.NotifyNotJT();
                }catch (Exception ex){
                    if (ex is RemotingException || ex is SocketException || ex is IOException || ex is InvalidOperationException)
                        Console.WriteLine("[Unavailable Worker] Couldn't talk with " + callerUrl);
                }
            }
            currentTerm = Math.Max(currentTerm, term);
            return isCallerJT;
        }

        private void workerNetConnect(string entrypointURL)
        {
            IWorker workerProxy;
            try
            {
                workerProxy = getRemoteWorker(entrypointURL);
                foreach (string url in workerProxy.getWorkersURL())
                    workersURL.Add(url);
                workersURL.Add(entrypointURL);
                broadcastToWorkers();
            }
            catch (Exception ex)
            {
                if (ex is RemotingException || ex is SocketException || ex is IOException)
                {
                    Console.WriteLine("[Broadcast] Couldn't broadcast to some workers, trying again");
                }
            }

        }

        private string GetPartner(string wUrl)
        {
            int i = 0;
            foreach(string url in workersURL){
                if (url.Equals(wUrl))
                {
                    Console.WriteLine("[Partner Info] Partner of " + wUrl + " is " + workersURL.ElementAt((i + 1) % workersURL.Count));
                    return workersURL.ElementAt((i + 1) % workersURL.Count);
                }
                i++;
            }
            return "";
        }

        private void broadcastToWorkers()
        {
            IWorker workerProxy;
            foreach (string url in workersURL)
            {
                Console.WriteLine("[Broadcast] Broadcasted To " + url);
                try{
                    workerProxy = getRemoteWorker(url);
                    workerProxy.addWorkerURL(getUrl());
                }
                catch (RemotingException){
                    Console.WriteLine("[Broadcast] Couldn't broadcast to " + url);
                }
            }
        }
        #endregion

        #region WorkersNet Methods

        public HashSet<string> getWorkersURL()
        {
            return workersURL;   
        }

        public void addWorkerURL(string url)
        {
            workersURL.Add(url);
        }

        public void setSubJobs(List<KeyValuePair<string, string>> unfinishdJobs, string url, int splitBegin, int splitEnd, int indexesCompleted)
        {
            subHalfOutput = unfinishdJobs;
            backingJob = new JobMap(getUrl(), splitBegin, splitEnd);
            backingUrl = url;
            backingIndexes = indexesCompleted;
        }
        #endregion

        #region Job Services
        public void SetOnHoldJobs(Queue<KeyValuePair<int, int>> _onHoldJobs)
        {
            while (true) { if (communicable) break; Thread.Sleep(100); }
            while (true) { if ((currJTStatus != JTStatus.ISJT) || ((currJTStatus == JTStatus.ISJT) && JTcommunicable)) break; Thread.Sleep(100); }
            if (currJTStatus != JTStatus.ISJT)
            {
                onHoldJobs = _onHoldJobs;
                hasAssignedJobs = true;
            }
        }

        public void SubmitJob(int _numOfSlices, int _inputSize, string _clientUrl)
        {
            while (true) { if (communicable) break; Thread.Sleep(100); }
            while (true) { if ((currJTStatus != JTStatus.ISJT) || ((currJTStatus == JTStatus.ISJT) && JTcommunicable)) break; Thread.Sleep(100); }
            try
            {
                stopWatch.Start();
                Console.WriteLine("[Client Interaction] Received Submitjob");
                IClient clientProxy = (IClient)Activator.GetObject(typeof(IClient), _clientUrl);
                if (clientProxy == null)
                {
                    throw new Exception("[Unavailable Client] Failed to conenct to Client");
                }
                else if (workersURL.Count == 0)
                {
                    InitJobTracking(_numOfSlices, _inputSize, _clientUrl, getUrl(), myTerm);
                }
                else if (!jtUrl.Equals(""))
                {
                    bool succeded = true;
                    try
                    {
                        getRemoteWorker(jtUrl).InitJobTracking(_numOfSlices, _inputSize, _clientUrl, getUrl(), currentTerm);
                    }
                    catch (Exception ex){
                        if (ex is RemotingException || ex is SocketException || ex is IOException){
                            Console.WriteLine("[Unavailable Worker] Couldn't talk with " + jtUrl);
                            succeded = false;
                        }
                    }
                    if(succeded)
                        return;
                }
                else
                {
                    Random rng = new Random();
                    bool ping = true;
                    int position = 0;
                    IWorker jobTracker = null;

                    do
                    {
                        position = rng.Next(0, workersURL.Count);
                        Console.WriteLine("[JT Decision] Choosing worker " + position + " to be JT");
                        try
                        {
                            jobTracker = getRemoteWorker(workersURL.ElementAt(position));
                            jobTracker.InitJobTracking(_numOfSlices, _inputSize, _clientUrl, getUrl(), currentTerm);
                        }
                        catch (Exception ex){
                            if (ex is RemotingException || ex is SocketException || ex is IOException){
                                Console.WriteLine("[Unavailable Worker] Couldn't talk with " + workersURL.ElementAt(position));
                                failedWorkers.Add(workersURL.ElementAt(position));
                                ping = false;
                            }
                        }
                    } while (!ping);
                }
            }
            catch (Exception ex){
                if (ex is RemotingException || ex is SocketException || ex is IOException){
                    Console.WriteLine("[Client Interaction] Job submission failed");
                }
            }
        }

        public void notifyProgress(string url, int percentage)
        {
            double elapsed = stopWatch.ElapsedMilliseconds / 1000;
            if (progresses.ContainsKey(url))
            {
                if (percentage == 0)
                    progresses[url] = new ProgressReport(elapsed, elapsed, percentage, backerUrl);
                else
                    progresses[url] = new ProgressReport(progresses[url].init, elapsed, percentage, backerUrl);
            }
            else if(percentage == 0)
                progresses.Add(url, new ProgressReport(elapsed, elapsed, percentage, backerUrl));
        }

        // Return the number of completed cicles of the worker being done
        public int TrackJob()
        {
            while (true) { if (communicable) break; Thread.Sleep(100); }
            while (true) { if ((currJTStatus != JTStatus.ISJT) || ((currJTStatus == JTStatus.ISJT) && JTcommunicable)) break; Thread.Sleep(100); }
            Console.WriteLine("\r\n--------------STATUS REPORT--------------");
            int percentage = 0;
            if (currJTStatus != JTStatus.ISJT && !jtUrl.Equals(""))
            {
                percentage = (int)Math.Round((float)indexesCompleted / (float)indexesToComplete * 100);
                Console.WriteLine("-> Work progress: " + percentage + "%");
                Console.WriteLine("-> Job tracker is at " + jtUrl);
            }
            if (currJTStatus == JTStatus.ISJT)
            {
                Console.WriteLine("-> Present nodes:");
                foreach (string workerUrl in workersURL)
                {
                    Console.WriteLine("-->" + workerUrl);
                }
                Console.WriteLine("-> Failed nodes:");
                foreach (string workerUrl in failedWorkers) {
                    Console.WriteLine("-->" + workerUrl);
                }
            }
            Console.WriteLine("--------------||--------------\r\n");
            return percentage;
        }

        public void NotifyNewJT(string callerUrl, int term)
        {
            if(!CheckTerm(callerUrl, term)) return;
            jtUrl = callerUrl;
        }

        public void CancelJob(string callerUrl, int term)
        {
            Console.WriteLine("[Job] Canceling job");
            while (true) { if (communicable) break; Thread.Sleep(100); }
            while (true) { if ((currJTStatus != JTStatus.ISJT) || ((currJTStatus == JTStatus.ISJT) && JTcommunicable)) break; Thread.Sleep(100); }
            if (!CheckTerm(callerUrl, term)) return;
            cancelJob = true;
        }

        public void NotifyNotJT()
        {
            currJTStatus = JTStatus.NONE;
        }

        public void GiveJob(int splitBegin, int splitEnd, string _clientUrl, string callerUrl, int term, string _backerUrl)
        {
            while (true) { if (communicable) break; Thread.Sleep(100); }
            while (true) { if ((currJTStatus != JTStatus.ISJT) || ((currJTStatus == JTStatus.ISJT) && JTcommunicable)) break; Thread.Sleep(100); }
            if (!isAvailable()) throw new RemotingException();
            if (!CheckTerm(callerUrl, term)) return;
            clientUrl = _clientUrl;
            backerUrl = _backerUrl;
            currentJob = new JobMap(getUrl(), splitBegin, splitEnd);
        }

        public static void ComputeJob()
        {
            computing = true;
            available = false;
            bool subWork = false;
            JobMap job = new JobMap();
            bool backingWorkerIsAlive = true;
            if (!backingUrl.Equals(""))
            {
                try
                {
                    Console.WriteLine("[Job Substitution] Checking if backing worker is alive");
                    getRemoteWorker(backingUrl).Ping();
                }
                catch (Exception ex){
                    if (ex is RemotingException || ex is SocketException || ex is IOException){
                        Console.WriteLine("[Job Substitution] " + backingUrl + "is dead, trying to replace him");
                        job = backingJob;
                        backingWorkerIsAlive = false;
                        subWork = true;
                    }
                }
                finally
                {
                    if (backingWorkerIsAlive && !forceJobSub)
                    {
                        if (currentJob.splitBegin == -1)
                        {
                            available = true;
                            computing = false;
                        }
                        else
                            job = currentJob;
                    }
                }
            }
            else
            {
                if (currentJob.splitBegin == -1)
                {
                    available = true;
                    computing = false;
                }
                else
                    job = currentJob;
            }
            if (!computing)
            {
                available = true;
                return;
            }
            getRemoteWorker(jtUrl).notifyProgress(getUrl(), 0);
            int splitBegin = job.splitBegin;
            int splitEnd = job.splitEnd;
            int currSplitBegin = splitBegin;
            bool notified33 = false;
            bool notified66 = false;
            if (subHalfOutput != null && subWork)
            {
                Console.WriteLine("[Job Substitution] Computing backing job");
                currSplitBegin += backingIndexes;
            }
            currentJob = job; 
            Console.WriteLine("[Job] Computing job");
            indexesCompleted = 0;
            if (subHalfOutput != null && subWork)
                indexesCompleted += splitEnd - currSplitBegin;
            indexesToComplete = Math.Max(1, splitEnd - currSplitBegin);

            Console.WriteLine("[Client Interaction] Asking from split " + splitBegin + " to index " + splitEnd + " from client " + clientUrl);
            IClient clientProxy = (IClient)Activator.GetObject(typeof(IClient), clientUrl);
            IList<string> inputSplit = clientProxy.getSplit(currSplitBegin, splitEnd);
            if (dll == null)
            {
                dll = clientProxy.getDLLSettings();
                mapperName = clientProxy.getMapperName();
                try
                {
                    assembly = Assembly.Load(dll);
                }
                catch (ArgumentNullException)
                {
                    Console.WriteLine("[DLL] Loading unsuccessful");
                    computing = false;
                    available = true;
                    return;
                }
                catch (BadImageFormatException)
                {
                    Console.WriteLine("[DLL] Loading unsuccessful");
                    computing = false;
                    available = true;
                    return;
                }
            }
            
            IList<KeyValuePair<string, string>> outputSplit = new List<KeyValuePair<string, string>>();
            if (subHalfOutput != null && subWork)
                ((List<KeyValuePair<string, string>>) outputSplit).AddRange(subHalfOutput);
            subHalfOutput = null;
            foreach (string input in inputSplit)
            {
                while (true) { if (!isSlowed) break; Thread.Sleep(100); }
                if (cancelJob)
                {
                    currentJob = new JobMap("", -1, -1);
                    available = true;
                    outputSplit.Clear();
                    notYetSent = true;
                    computing = false;
                    cancelJob = false;
                    if(subWork)
                        forceJobSub = false;
                }
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.IsClass == true)
                    {
                        if (type.FullName.EndsWith("." + mapperName))
                        {
                            // create an instance of the object
                            try
                            {
                                object ClassObj = Activator.CreateInstance(type);
                                // Dynamically Invoke the method
                                object[] args = new object[] { input };
                                object resultObject = type.InvokeMember("Map",
                                    BindingFlags.Default | BindingFlags.InvokeMethod,
                                        null,
                                        ClassObj,
                                        args);
                                ((List<KeyValuePair<string, string>>) outputSplit).AddRange((IList<KeyValuePair<string, string>>)resultObject);
                            }
                            catch (Exception)
                            {
                                Console.WriteLine("[Job] Couldn't initialize map function, exiting...");
                                return;
                            }
                        }
                    }
                }
                indexesCompleted++;
                int progress = (int)Math.Round((float)indexesCompleted / (float)indexesToComplete * 100);
                if ((progress >= 33) && !notified33)
                {
                    getRemoteWorker(jtUrl).notifyProgress(getUrl(), progress);
                    notified33 = true;
                }
                if ((progress >= 66) && !notified66)
                {
                    getRemoteWorker(jtUrl).notifyProgress(getUrl(), progress);
                    notified66 = true;
                }
                if (progress >= 50 && progress <= 80 && notYetSent)
                {  //if half of the job is done, send the result to the partner/sub
                    if (!subWork && !backerUrl.Equals(""))
                    {
                        try
                        {
                            Console.WriteLine("[Job] Halfway, going to send backup to partner at " + backerUrl);
                            IWorker subWorker = getRemoteWorker(backerUrl);
                            subWorker.setSubJobs((List<KeyValuePair<string, string>>)outputSplit, getUrl(), splitBegin, splitEnd, indexesCompleted);
                            notYetSent = false;
                        }
                        catch (Exception ex){
                            if (ex is RemotingException || ex is SocketException || ex is IOException)
                                Console.WriteLine("[Job] Couldn't backup");
                        }
                        Console.WriteLine("[Job] Backed up successfully to " + backerUrl);
                    }
                }
            }
            RemoteCliAsyncDelegate RemoteDel = new RemoteCliAsyncDelegate(clientProxy.outputSplit);
            RemoteDel.BeginInvoke(outputSplit, splitBegin, splitEnd, jtUrl, null, null);
            Console.WriteLine("[Client Interaction] Output sent to client " + splitBegin + " ; " + splitEnd);
            currentJob = new JobMap("", -1, -1);
            outputSplit.Clear();
            if (subWork)
            {
                backingUrl = "";
                backingJob = new JobMap("", -1, -1);
                backingIndexes = 0;
                forceJobSub = false;
            }
            available = true;
            computing = false;
            notYetSent = true;
        }

        public void FinishedJob()
        {
            while (true) { if (communicable) break; Thread.Sleep(100); }
            while (true) { if ((currJTStatus!=JTStatus.ISJT)||((currJTStatus == JTStatus.ISJT) && JTcommunicable)) break; Thread.Sleep(100); }
            if (currJTStatus == JTStatus.ISJT)
            {
                foreach (string wUrl in workersURL)
                {
                    IWorker wk;
                    try
                    {
                        wk = getRemoteWorker(wUrl);
                        wk.FinishedJob();
                    }
                    catch (Exception ex)
                    {
                        if (ex is RemotingException || ex is SocketException || ex is IOException)
                            Console.WriteLine("[Unavailable Worker] Couldn't talk with " + jtUrl);
                    }
                }
            }
            dll = null;
        }

        public void InitJobTrackingSubstitution(string _jtUrl, int _numOfSlices, int _inputSize, string _clientUrl, string callerUrl, int term)
        {
            while (true) { if (communicable) break; Thread.Sleep(100); }
            while (true) { if ((currJTStatus != JTStatus.ISJT) || ((currJTStatus == JTStatus.ISJT) && JTcommunicable)) break; Thread.Sleep(100); }
            if(!CheckTerm(callerUrl, term)) return;
            if (currJTStatus != JTStatus.ISJT)
            {
				currJTStatus = JTStatus.ISJTSUB;
                Console.WriteLine("[JT Sub] Choosen as JT substitute");
                clientUrl = _clientUrl;
                inputSize = _inputSize;
                numSlices = _numOfSlices;
                jtUrl = _jtUrl;
            }
        }


        public void InitJobTracking(int _numOfSlices, int _inputSize, string _clientUrl, string callerUrl, int term)
        {
            while (true) { if (communicable) break; Thread.Sleep(100); }
            while (true) { if ((currJTStatus != JTStatus.ISJT) || ((currJTStatus == JTStatus.ISJT) && JTcommunicable)) break; Thread.Sleep(100); }
            if(!CheckTerm(callerUrl, term)) return;
            Console.WriteLine("[JT] Job initialization");
            numSlices = _numOfSlices;
            inputSize = _inputSize;
            bool wasAlreadyJt = jtUrl.Equals(getUrl());
            if (!wasAlreadyJt)
            {
                myTerm = currentTerm + 1;
                currentTerm = myTerm;
                currJTStatus = JTStatus.ISJT;
                jtUrl = getUrl();
                stopWatch.Start();
            }
            clientUrl = _clientUrl;
            if (!wasAlreadyJt)
            {
                Random rng = new Random();
                int position = 0;
                IWorker jobTrackerSubstitute = null;
                IWorker wk;
                foreach (string workerUrl in workersURL)
                {
                    try
                    {
                        wk = getRemoteWorker(workerUrl);
                        newJTDelegate del = new newJTDelegate(wk.NotifyNewJT);
                        del.BeginInvoke(getUrl(), myTerm, null, null);
                    }
                    catch (Exception ex){
                        if (ex is RemotingException || ex is SocketException || ex is IOException)
                            Console.WriteLine("[Unavailable Worker] Couldn't talk with " + workerUrl);
                    }
                }
                if (workersURL.Count > 0)
                {
                    bool ping;
                    do
                    {
                        ping = true;
                        position = rng.Next(0, workersURL.Count);
                        Console.WriteLine("[JT Sub Decision] Choosing worker " + position + " to be JT substitute");
                        try
                        {
                            jobTrackerSubstitute = getRemoteWorker(workersURL.ElementAt(position));
                            jobTrackerSubstitute.InitJobTrackingSubstitution(getUrl(), _numOfSlices, _inputSize, _clientUrl, getUrl(), myTerm);
                            jtSubUrl = workersURL.ElementAt(position);
                        }
                        catch (Exception ex){
                            if (ex is RemotingException || ex is SocketException || ex is IOException){
                                Console.WriteLine("[Unavailable Worker] Couldn't talk with " + workersURL.ElementAt(position));
                                failedWorkers.Add(workersURL.ElementAt(position));
                                ping = false;
                            }
                        }
                    } while (!ping && workersURL.Count > 0);
                }
            }

            if (hasAssignedJobs)
            {
                Console.WriteLine("[JT] Already have jobs on hold");
                return;
            }

            int indexBegin;
            int indexEnd;
            Console.WriteLine("[JT] Preparing to enqueue: " + numSlices + " slices");
            for (int i = 0; i < numSlices; i++)
            {
                indexBegin = i * (inputSize / numSlices) + ((inputSize / numSlices > 0 && i > 0) ? 1 : 0);
                indexEnd = ((i + 1) * (inputSize / numSlices)) - 1 + (inputSize / numSlices > 0 ? 1 : 0);
                if (indexEnd >= inputSize)
                    indexEnd--;
                onHoldJobs.Enqueue(new KeyValuePair<int, int>(indexBegin, indexEnd));
            }
        }

        public bool Ping()
        {
            while (true) { if (communicable) break; Thread.Sleep(100); }
            while (true) { if ((currJTStatus != JTStatus.ISJT) || ((currJTStatus == JTStatus.ISJT) && JTcommunicable)) break; Thread.Sleep(100); }
            return true;
        }

        public void showElapsedTime()
        {
            while (true) { if (communicable) break; Thread.Sleep(100); }
            while (true) { if ((currJTStatus != JTStatus.ISJT) || ((currJTStatus == JTStatus.ISJT) && JTcommunicable)) break; Thread.Sleep(100); }
            stopWatch.Stop();
            Console.WriteLine("[Job] Job done in " + (stopWatch.ElapsedMilliseconds / 1000) + " secs");
        }

        public bool isAvailable(){
            while (true) { if (communicable) break; Thread.Sleep(100); }
            while (true) { if ((currJTStatus != JTStatus.ISJT) || ((currJTStatus == JTStatus.ISJT) && JTcommunicable)) break; Thread.Sleep(100); }
            return (currentJob.splitBegin==-1) && available;
        }

        public void Slow(int secToSlow) {
            isSlowed = true;
            slowTimer = new System.Timers.Timer(secToSlow * 1000);
            slowTimer.Enabled = true;
            slowTimer.Elapsed += OnTimedSlowEvent;
        }

        public void FreezeW()
        {
            Console.WriteLine("[Failures] Freezing worker");
            communicable = false;
            isSlowed = true;
        }

        public void UnfreezeW()
        {
            Console.WriteLine("[Failures] Unfreezing worker");
            communicable = true;
            isSlowed = false;
        }

        public void FreezeC()
        {
            Console.WriteLine("[Failures] Freezing jt communication");
            JTcommunicable = false;
        }

        public void UnfreezeC()
        {
            Console.WriteLine("[Failures] Unfreezing jt communication");
            JTcommunicable = true;
        }

        public void ForceJobSub(string callerUrl, int term)
        {
            while (true) { if (communicable) break; Thread.Sleep(100); }
            while (true) { if ((currJTStatus != JTStatus.ISJT) || ((currJTStatus == JTStatus.ISJT) && JTcommunicable)) break; Thread.Sleep(100); }
            forceJobSub = true;
        }

        #endregion

        #region Clock
        private void OnTimedSlowEvent(Object source, ElapsedEventArgs e)
        {
            isSlowed = false;
            slowTimer.Close();
            slowTimer.Enabled = false;
        }

        /*public bool WasReseted()
        {
            return reseted;
        }*/

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            if (isOnTimeEvent) return;
            else isOnTimeEvent = true;

            if (currJTStatus == JTStatus.ISJT && JTcommunicable)
            {
                float averageSpeed = 0f;
                float sumSpeed = 0f;
                int numProgresses = 0;
                foreach (KeyValuePair<string, ProgressReport> progress in progresses)
                {
                    /*Console.WriteLine("Progress of " + progress.Key + " was " + progress.Value.percentage +
                        " began at " + progress.Value.init + " timestamped at " + progress.Value.timestamp);*/
                    if (progress.Value.timestamp != progress.Value.init)
                    {
                        numProgresses++;
                        sumSpeed += (float)progress.Value.percentage / (float)(progress.Value.timestamp - progress.Value.init);
                    }
                }
                if (numProgresses > 0)
                {
                    Console.WriteLine("\r\n------------Progress Report------------");
                    averageSpeed = sumSpeed / numProgresses;
                    //Console.WriteLine("Average speed: " + averageSpeed);
                    Console.WriteLine("List of progresses");
                    foreach (KeyValuePair<string, ProgressReport> progress in progresses)
                    {
                        if (progress.Value.timestamp != progress.Value.init){
                            int relativeSpeed = (int)Math.Round((100 * (float)progress.Value.percentage / (float)(progress.Value.timestamp - progress.Value.init))
                                /averageSpeed);
                            Console.WriteLine("-> Node " + progress.Key + " is at " + relativeSpeed + " speed from the average");
                            if (relativeSpeed < 45 && progress.Value.percentage > 60)
                            {
                                try
                                {
                                    getRemoteWorker(progress.Key).CancelJob(getUrl(), myTerm);
                                }
                                catch (Exception ex){
                                    if (ex is RemotingException || ex is SocketException || ex is IOException){
                                        progresses.Remove(progress.Key);
                                        Console.WriteLine("-> Node " + progress.Key + " is not available for canceling job");
                                        continue;
                                    }
                                }
                                try
                                {
                                    Console.WriteLine("[Slow Analysis] Forcing job substitution");
                                    getRemoteWorker(progress.Value.backerUrl).ForceJobSub(getUrl(), myTerm);
                                }
                                catch (Exception ex){
                                    if (ex is RemotingException || ex is SocketException || ex is IOException){
                                        Console.WriteLine("[Unavailable Worker] Backup node is not available");
                                        continue;
                                    }
                                }
                                progresses.Remove(progress.Key);
                            }else if (relativeSpeed < 20)
                            {
                                try
                                {
                                    getRemoteWorker(progress.Key).CancelJob(getUrl(), myTerm);
                                }
                                catch (Exception ex){
                                    if (ex is RemotingException || ex is SocketException || ex is IOException){
                                        progresses.Remove(progress.Key);
                                        Console.WriteLine("-> Node " + progress.Key + " is not available for canceling job");
                                        continue;
                                    }
                                }
                                try
                                {
                                    Console.WriteLine("[Job Substitution] Forcing job substitution");
                                    getRemoteWorker(progress.Value.backerUrl).ForceJobSub(getUrl(), myTerm);
                                }
                                catch (Exception ex){
                                    if (ex is RemotingException || ex is SocketException || ex is IOException){
                                        Console.WriteLine("[Unavailable Worker] Backup node is not available");
                                        continue;
                                    }
                                }
                                progresses.Remove(progress.Key);
                            }
                        }
                    }
                    Console.WriteLine("------------||----------\r\n");
                }
                IList<string> availableWorkers = new List<string>();
                IWorker workerProxy = null;
                foreach (string workerURL in workersURL)
                {
                    try
                    {
                        workerProxy = getRemoteWorker(workerURL);
                        workerProxy.Ping();
                    }
                    catch (Exception ex)
                    {
                        if (ex is RemotingException || ex is SocketException || ex is IOException)
                        {
                            Console.WriteLine("[Unavailable Worker] Couldn't talk with " + workerURL);
                            failedWorkers.Add(workerURL);
                            continue;
                        }
                    }
                    try
                    {
                        if (workerProxy.isAvailable())
                        {
                            if (failedWorkers.Contains(workerURL))
                            {
                                bool succeded = true;
                                try
                                {
                                    workerProxy.CancelJob(getUrl(), myTerm);
                                }
                                catch (Exception ex)
                                {
                                    if (ex is RemotingException || ex is SocketException || ex is IOException)
                                    {
                                        Console.WriteLine("[Unavailable Worker] Couldn't talk with " + workerURL);
                                        succeded = false;
                                    }
                                }
                                finally
                                {
                                    if (succeded) failedWorkers.Remove(workerURL);
                                }
                            }
                            availableWorkers.Add(workerURL);
                        }
                    }
                    catch (Exception ex){
                        if (ex is RemotingException || ex is SocketException || ex is IOException){
                            Console.WriteLine("[Unavailable Worker] Couldn't talk with " + workerURL);
                            failedWorkers.Add(workerURL);
                            continue;
                        }
                    }
                }
                if (failedWorkers.Count >= workersURL.Count)
                {
                    if (onHoldJobs.Count != 0 && currentJob.splitBegin == -1)
                    {
                        KeyValuePair<int, int> kvp = onHoldJobs.Dequeue();
                        currentJob = new JobMap(getUrl(), kvp.Key, kvp.Value);
                    }
                }
                foreach (string availableWorkerURL in availableWorkers)
                {
                    try
                    {
                        IWorker availableWorker = getRemoteWorker(availableWorkerURL);
                        availableWorker.Ping();
                        if (!availableWorker.isAvailable()) continue;
                        if (onHoldJobs.Count != 0)
                        {
                            RemoteGiveJobDelegate RemoteDel = new RemoteGiveJobDelegate(availableWorker.GiveJob);
                            KeyValuePair<int, int> job = onHoldJobs.Dequeue();
                            RemoteDel.BeginInvoke(job.Key, job.Value, clientUrl, getUrl(), myTerm, GetPartner(availableWorkerURL), null, null);
                            Console.WriteLine("[JobTracker] " + "Giving split begin " + job.Key + " ; end " + job.Value + " to worker " + availableWorkerURL);
                        }
                    }
                    catch (Exception ex){
                        if (ex is RemotingException || ex is SocketException || ex is IOException){
                            Console.WriteLine("[Unavailable Worker] Couldn't talk with " + availableWorkerURL);
                            failedWorkers.Add(availableWorkerURL);
                        }
                    }
                }
                try
                {
                    IWorker jtSub = getRemoteWorker(jtSubUrl);
                    jtSubOnHoldJobsDelegate jtOnHoldJobs = new jtSubOnHoldJobsDelegate(jtSub.SetOnHoldJobs);
                    jtOnHoldJobs.BeginInvoke(onHoldJobs, null, null);
                }
                catch (RemotingException)
                {
                    Console.WriteLine("[JT Replication] Couldn't talk with JT subtitute " + jtSubUrl);
                }
                finally
                {
                    isOnTimeEvent = false;
                }
            }
            else if (currJTStatus == JTStatus.ISJTSUB && communicable && !jtUrl.Equals(""))
            {
                try
                {
                    Console.WriteLine("[JT Subtitute] Checking if JT is alive");
                    IWorker jobTracker = getRemoteWorker(jtUrl);
                    jobTracker.Ping();

                }
                catch (Exception ex){
                        if (ex is RemotingException || ex is SocketException || ex is IOException){
                            Console.WriteLine("[JT Substitute] Couldn't talk with JT " + jtUrl);
                            InitJobTracking(numSlices, inputSize, clientUrl, getUrl(), currentTerm);
                            isOnTimeEvent = false;
                            return;
                        }
                }
            }if(currJTStatus == JTStatus.NONE || currJTStatus == JTStatus.ISJTSUB || currentJob.splitBegin !=-1){
                if (!computing)
                {
                    isOnTimeEvent = false;
                    ComputeJob();
                    return;
                }
            }
            isOnTimeEvent = false;
        }
        #endregion
    }
}
