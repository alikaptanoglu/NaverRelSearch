using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Winner.Service
{
    class UIService
    {
        // DataGridView Row 스퀀스 처리
        public static void AutoSequence(DataGridView dataGridView)
        {
            int cellnum = 0;
            int rownum = 0;

            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                cellnum = cellnum + 1;
                dataGridView.Rows[rownum].Cells[0].Value = cellnum;
                rownum = rownum + 1;
            }
        }

        // DataGridView Row 최상단으로 이동
        public static void DataGridViewRowMoveTop(DataGridView dataGridView)
        {
            if (dataGridView.RowCount > 0)
            {
                if (dataGridView.SelectedRows.Count > 0)
                {
                    int rowCount = dataGridView.Rows.Count;
                    int index = dataGridView.SelectedCells[0].OwningRow.Index;

                    if (index == 0)
                    {
                        return;
                    }
                    DataGridViewRowCollection rows = dataGridView.Rows;
                    DataGridViewRow targetRow = rows[index];
                    List<DataGridViewRow> list = new List<DataGridViewRow>();

                    list.Add(targetRow);
                    for (int i = 0; i < rows.Count; i++)
                    {
                        if (i == index) continue;
                        list.Add(rows[i]);
                    }

                    dataGridView.Rows.Clear();

                    foreach (DataGridViewRow row in list)
                    {
                        dataGridView.Rows.Add(row);

                    }

                    dataGridView.ClearSelection();
                    dataGridView.Rows[0].Selected = true;
                }
            }

            AutoSequence(dataGridView);

        }

        // DataGridView Row 최하단으로 이동
        public static void DataGridViewRowMoveBottom(DataGridView dataGridView)
        {
            if (dataGridView.RowCount > 0)
            {
                if (dataGridView.SelectedRows.Count > 0)
                {
                    int rowCount = dataGridView.Rows.Count;
                    int index = dataGridView.SelectedCells[0].OwningRow.Index;

                    if (index == (rowCount - 1)) // include the header row
                    {
                        return;
                    }
                    DataGridViewRowCollection rows = dataGridView.Rows;
                    DataGridViewRow targetRow = rows[index];
                    List<DataGridViewRow> list = new List<DataGridViewRow>();

                    for (int i = 0; i < rows.Count; i++)
                    {
                        if (i == index) continue;
                        list.Add(rows[i]);
                    }
                    list.Add(targetRow);

                    dataGridView.Rows.Clear();

                    foreach (DataGridViewRow row in list)
                    {
                        dataGridView.Rows.Add(row);

                    }

                    dataGridView.ClearSelection();
                    dataGridView.Rows[rowCount - 1].Selected = true;
                }
            }
        }

        // DataGridView Row 하단으로 이동
        public static void DataGridViewRowMoveDown(DataGridView dataGridView)
        {
            if (dataGridView.RowCount > 0)
            {
                if (dataGridView.SelectedRows.Count > 0)
                {
                    int rowCount = dataGridView.Rows.Count;
                    int index = dataGridView.SelectedCells[0].OwningRow.Index;

                    if (index == (rowCount - 1)) // include the header row
                    {
                        return;
                    }
                    DataGridViewRowCollection rows = dataGridView.Rows;

                    // remove the next row and add it in front of the selected row.
                    DataGridViewRow nextRow = rows[index + 1];
                    rows.Remove(nextRow);
                    nextRow.Frozen = false;
                    rows.Insert(index, nextRow);
                    dataGridView.ClearSelection();
                    dataGridView.Rows[index + 1].Selected = true;
                }
            }
        }

        // DataGridView Row 상단으로 이동
        public static void DataGridViewRowMoveUp(DataGridView dataGridView)
        {
            if (dataGridView.RowCount > 0)
            {
                if (dataGridView.SelectedRows.Count > 0)
                {
                    int rowCount = dataGridView.Rows.Count;
                    int index = dataGridView.SelectedCells[0].OwningRow.Index;

                    if (index == 0)
                    {
                        return;
                    }
                    DataGridViewRowCollection rows = dataGridView.Rows;



                    // remove the previous row and add it behind the selected row.
                    DataGridViewRow prevRow = rows[index - 1];
                    rows.Remove(prevRow);
                    prevRow.Frozen = false;
                    rows.Insert(index, prevRow);
                    dataGridView.ClearSelection();
                    dataGridView.Rows[index - 1].Selected = true;
                }
            }
        }

    }
}
