using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PADIMapInterfaces
{
    public interface IMapper
    {
        IList<KeyValuePair<string, string>> Map(string fileLine);
    }

    public interface IMapperTransfer
    {
        bool SendMapper(byte[] code, string className, IList<string> input);
    }

    public interface IWorker {
        // Worker methods
        // WorkersNet methods
        HashSet<string> getWorkersURL();
        void addWorkerURL(string url);
        //void setSubWorker(string url);
        void setSubJobs(List<KeyValuePair<string, string>> unfinishdJobs, string backerUrl, int splitBegin, int splitEnd, int indexes);
        //void updateProgresses(Dictionary<string, ProgressReport> progresses);
        void notifyProgress(string url, int percentage);
        //void SetWorkerPartners(Dictionary<string, string> workerPartners);
        //void removeWorkerURL(string url);
        // Job controllers
        void GiveJob(int splitBegin, int splitEnd, string clientUrl, string callerUrl, int term, string backerUrl);
        void CancelJob(string callerUrl, int term);
        void SubmitJob(int numSlices, int inputSize, string clientUrl);
        bool Ping();
        void InitJobTracking(int numOfSlices, int inputSize, string clientUrl, string callerUrl, int term);
        void InitJobTrackingSubstitution(string url, int numOfSlices, int inputSize, string clientUrl, string callerUrl, int term);
        bool isAvailable();
        void SetOnHoldJobs(Queue<KeyValuePair<int, int>> onHoldJobs);
        // PuppetMaster commands
        void showElapsedTime();
        //void SetJobMap(IList<JobMap> jobMaps);
        int TrackJob();
        void Slow(int secToSlow);
        void FreezeW();
        void UnfreezeW();
        void FreezeC();
        void UnfreezeC();
        void NotifyNotJT();
        void NotifyNewJT(string callerUrl, int term);
        void ForceJobSub(string callerUrl, int term);
        void FinishedJob();
        bool WasReseted();
    }

    public interface IClient {
        IList<string> getSplit(int indexBegin, int indexEnd);
        void outputSplit(IList<KeyValuePair<string, string>> result, int indexBegin, int indexEnd, string jtUrl);
        byte[] getDLLSettings();
        string getMapperName();
        bool HasReceivedSplit(int indexBegin, int indexEnd);
    }

    public interface IPuppetMaster
    {
        bool CreateWorker(int id, string workerURL);
        bool CreateWorkerEntry(int id, string workerURL, string entryPoint);
    }
}
