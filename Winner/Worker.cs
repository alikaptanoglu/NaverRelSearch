using Naver.SearchAd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Winner
{
    class WorkerManager
    {
        private Main UI;
        private List<DataGridViewRow> SLOTS = new List<DataGridViewRow>();
        private static LogManager logManager;
        private static SQLite SQL = new SQLite();
        Thread workThread;
        private static Dictionary<string, string> config;

        public WorkerManager(Main UI)
        {
            this.UI = UI;
        }

        // 작업 실행
        public void Start()
        {
            workThread = new Thread(new ThreadStart(Run));
            workThread.Start();           
        }

        void Run()
        {            
            // 작업 준비
            InitializeWork();

            // 작업실행
            WorkStart();
        }

        private void WorkStart()
        {
            ManualResetEvent[] doneEvents = new ManualResetEvent[SLOTS.Count];

            for (int i = 0; i < SLOTS.Count; i++)
            {
                doneEvents[i] = new ManualResetEvent(false);
                Worker worker = new Worker(SLOTS[i], doneEvents[i]);

                ThreadPool.UnsafeQueueUserWorkItem(worker.ThreadPoolCallback, i);                
            }

            foreach (WaitHandle _doneEvent in doneEvents)
            {
                WaitHandle.WaitAny(new WaitHandle[] { _doneEvent });
            }            


            logManager.AppendLog("All calculations are complete.");
        }

        // 작업 초기화
        private void InitializeWork()
        {
           DataGridViewRowCollection rows  = UI.GetDataGridViewRows();
           foreach( DataGridViewRow row in rows)
           {
                SLOTS.Add(row);
           }

            List<Configuration> configs = SQL.SelectAllConfigurationByOwner(Configuration.Default);
            config = Configuration.ConvertObjectToMap(configs);
        }

        public void SetLogManager(LogManager log)
        {
            logManager = log;
        }


        // 작업 종료
        public void Stop()
        {
            //workThread.Abort();
        }

        class Worker
        {
            private DataGridViewRow dataGridViewRow;
            private ManualResetEvent _doneEvent;

            public Worker(DataGridViewRow dataGridViewRow, ManualResetEvent _doneEvent)
            {
                this.dataGridViewRow = dataGridViewRow;
                this._doneEvent = _doneEvent;
            }

            public void ThreadPoolCallback(object threadContext)
            {
                int threadIndex = (int)threadContext;
                doWork();
                _doneEvent.Set();
            }

            private void doWork()
            {
                try
                {
                    string slotId = (string)dataGridViewRow.Cells[Main.HEADER_SLOT].Value;
                    List<LogicItem> items = SQL.SelectAllLogicItemsBySlotId(slotId);
                    List<LogicInput> inputs = SQL.SelectAllLogicInputsByLogicId(items[0].logicId);
                    Dictionary<string, string> inputMap = LogicInput.ConvertObjectToMap(inputs);

                    if (items != null)
                    {
                        Command command = new Command(items, inputMap);
                        command.SetLogManager(logManager);
                        command.doCommand();
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }                                    
            }
        }

    }
}
