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
        
        private static LogManager logManager;
        private static SQLite SQL = new SQLite();
        Thread workThread;
        private static Dictionary<string, string> config;
        public int MAX_WORKER;        
        Thread[] threads;
        bool isConcel;

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
            try
            {
                // 작업 준비
                InitializeWork();

                // 작업실행
                WorkStart();
            }
            catch (Exception e)
            {
                foreach (Thread th in threads)
                {
                    th.Abort();
                }
            }            
        }

        private void WorkStart()
        {
            Queue<DataGridViewRow> SLOTS = new Queue<DataGridViewRow>();
            isConcel = false;

            while (true)
            {
                DataGridViewRowCollection rows = UI.GetDataGridViewRows();                

                foreach (DataGridViewRow row in rows)
                {
                    // 문제생기면 디비에서 가져오는 걸로..
                    if (int.Parse((string)row.Cells[Main.HEADER_TO_COUNT].Value) > int.Parse((string)row.Cells[Main.HEADER_CURR_COUNT].Value))
                    {
                        SLOTS.Enqueue(row);
                    }                        
                }

                if (SLOTS.Count != 0)
                {                    
                    while (SLOTS.Count != 0 && !isConcel)
                    {
                        MAX_WORKER = SLOTS.Count < MAX_WORKER ? SLOTS.Count : MAX_WORKER;

                        ManualResetEvent[] doneEvents = new ManualResetEvent[MAX_WORKER];
                        threads = new Thread[ MAX_WORKER];

                        for (int i = 0; i < MAX_WORKER; i++)
                        {
                            doneEvents[i] = new ManualResetEvent(false);
                            Worker worker = new Worker(UI, SLOTS.Dequeue(), doneEvents[i]);

                            threads[i] =  new Thread(new ThreadStart(worker.doProcess));
                            threads[i].Start();
                        }

                        foreach (WaitHandle _doneEvent in doneEvents)
                        {
                            WaitHandle.WaitAny( new WaitHandle[] { _doneEvent });
                        }
                    }

                    ComplateWork();
                }
                else
                {
                    FinishWork();
                    break;
                }                               
            }            
        }

        private void ComplateWork()
        {
            // 아이피 변경
            //ChangeIP();
            //CahngeMAC();
        }

        private void CahngeMAC()
        {
            //throw new NotImplementedException();
        }

        private void ChangeIP()
        {                        
            MobileUtils.EnAbleAirPlainMode();
            MobileUtils.DisAbleAirPlainMode();
                                                            
            UI.BeginInvoke(new Action(() => {                
                UI.setExternalAddress(CommonUtils.GetExternalIPAddress());
            }));            
        }

        private void FinishWork()
        {
            UI.BeginInvoke(new Action(() => {
                UI.Stop();
            }));
        }

        // 작업 초기화
        private void InitializeWork()
        {
            List<Configuration> configs = SQL.SelectAllConfigurationByOwner(Configuration.Default);
            config = Configuration.ConvertObjectToMap(configs);

            if (config[Configuration.WORK_DEFAULT].Equals("PARALLEL"))
            {
               MAX_WORKER =  int.Parse( config[Configuration.WORK_PARALLEL_COUNT]);
            }
            else
            {
               MAX_WORKER = 1;
            }
        }

        public void SetLogManager(LogManager log)
        {
            logManager = log;
        }


        // 작업 종료
        public void Stop()
        {                     
            isConcel = true;
            workThread.Abort();            
        }

        class Worker
        {
            private DataGridViewRow dataGridViewRow;
            private ManualResetEvent _doneEvent;
            private Main UI;
            private ManualResetEvent manualResetEvent;
            private Command command;

            public Worker(Main UI, DataGridViewRow dataGridViewRow, ManualResetEvent _doneEvent)
            {
                this.UI = UI;
                this.dataGridViewRow = dataGridViewRow;
                this._doneEvent = _doneEvent;
            }

            public void doProcess()
            {
                try
                {
                    DoWork();
                    ComplateWork();
                    _doneEvent.Set();
                }
                catch (Exception e)
                {
                    logManager.AppendLog(e.Message);
                    logManager.AppendLog(e.StackTrace);
                    command.Finish();                    
                }
            }

            private void ComplateWork()
            {
                string slotId = (string)dataGridViewRow.Cells[Main.HEADER_SLOT].Value;

                UI.BeginInvoke(new Action(() => {
                    dataGridViewRow.Cells[Main.HEADER_CURR_COUNT].Value = (int.Parse((string)dataGridViewRow.Cells[Main.HEADER_CURR_COUNT].Value) + 1).ToString();
                }));

                SQL.UpdateSlotCurrCount(slotId);
            }

            private void DoWork()
            {
                string slotId = (string)dataGridViewRow.Cells[Main.HEADER_SLOT].Value;
                List<LogicItem> items = SQL.SelectAllLogicItemsBySlotId(slotId);
                List<LogicInput> inputs = SQL.SelectAllLogicInputsByLogicId(items[0].logicId);
                Dictionary<string, string> inputMap = LogicInput.ConvertObjectToMap(inputs);

                if (items != null)
                {
                    command = new Command(items, inputMap);
                    command.SetLogManager(logManager);
                    command.doCommand();
                    command.Finish();
                }
            }
        }

    }
}
