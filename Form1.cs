using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Xml;
using System.Reflection;
using System.IO;

namespace TestGLValidation
{
    public partial class Form1 : Form
    {
        private scanData _taxdata = new scanData();
        private string testXmlLocation = @"C:\ProSystem fx Scan\ProSystem fx Scan Shared\Export\2012 John Smith (#23456)-1.xml";
        public Form1()
        {
            InitializeComponent();
            DisplayForm();
            dgvGainLoss.CellEndEdit += new DataGridViewCellEventHandler(dgvGainLoss_CellEndEdit);
        }
       

        private Trade GetTrade(DataGridViewRow row)
        {
            DataRow dr = GetDataRow(row);
            if (dr == null || dr.IsNull("Trade"))
                return null;

            return dr["Trade"] as Trade;
        }

        private DataRow GetDataRow(DataGridViewRow row)
        {
            if (row == null)
                return null;
            return (row.DataBoundItem as DataRowView).Row;
        }

        private void DisplayForm()
        {           
            var trades = GetTrades();

            DataTable dtForm = new DataTable();
            dtForm.Columns.Add("Trade", typeof(Trade));
            dtForm.Columns.Add("AcctNum");
            dtForm.Columns.Add("Description");
            dtForm.Columns.Add("Shares");
            dtForm.Columns.Add("SalesPrice");
            dtForm.Columns.Add("Cost");
            dtForm.Columns.Add("Acquired");
            dtForm.Columns.Add("Sold");
            dtForm.Columns[0].ColumnMapping = MappingType.Hidden;
            dtForm.BeginLoadData();
            foreach (var trade in trades)
            {
                DataRow drField = dtForm.NewRow();
                drField["Trade"] = trade;
                foreach (var tradeItem in trade.TradeItems)
                {
                    if (tradeItem.Name == "AcctNum")
                    {
                        drField["AcctNum"] = tradeItem;
                    }                   
                }
                dtForm.Rows.Add(drField);
            }
            dtForm.EndLoadData();
            

           // DataView dvForm = new DataView(dtForm);
            dgvGainLoss.DataSource = dtForm;
           
            //dgvGainLoss.CurrentCell = this.dgvGainLoss[1, 0];
        }
        
        private List<Trade> GetTrades()
        {
            //TODO: validate the xml before doing all these
            List<Trade> trades = new List<Trade>();
            
            var ser = new XmlSerializer(typeof(scanData));
            using (var reader = XmlReader.Create(testXmlLocation))
            {
                _taxdata = (scanData)ser.Deserialize(reader);
            }
            var tradeForms = _taxdata.forms.Where(fr => fr.shortName.Equals("IFDSGL") && fr.group != null);
            foreach (scanDataFormsForm tradeForm in tradeForms)
            {
                var pageNumber = tradeForm.pageNumber;
                foreach (var tradeGroup in tradeForm.group)
                {
                    List<TradeItem> tradeitems = new List<TradeItem>();
                    foreach (var tradeField in tradeGroup.field)
                    {
                        tradeitems.Add(new TradeItem(tradeField));
                    }
                    trades.Add(new Trade() { TradeItems = tradeitems });
                }

            }
            return trades;
        }

        private void dgvGainLoss_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewRow row = dgvGainLoss.Rows[e.RowIndex];
            string currentValue = dgvGainLoss[e.ColumnIndex, e.RowIndex].Value.ToString();
            Trade tradeData = GetTrade(row);
            string columnHeader = dgvGainLoss.Columns[e.ColumnIndex].HeaderText;
            TradeItem item = tradeData.TradeItems.FirstOrDefault(ti => ti.Name == columnHeader);
            item.SetValue(currentValue);
           //get column header
            //get property   to set



        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(scanData));
            using (TextWriter writer = new StreamWriter(testXmlLocation))
            {
                serializer.Serialize(writer, _taxdata);
            } 

        }
    }

    //this is a test class
    public class Trade
    {
        public List<TradeItem> TradeItems { get; set; }
    }
    public class TradeDataLocation
    {
        public TradeDataLocation(int left, int right, int top, int bottom)
        {
            Left = left;
            Right = right;
            Top = top;
            Bottom = bottom;
        }

        public int Left { get; set; }
        public int Right { get; set; }
        public int Top { get; set; }
        public int Bottom { get; set; }
    }
    public class TradeItem
    {
        private scanDataFormsFormGroupField _field;
        public TradeItem(scanDataFormsFormGroupField field)
        {
            _field = field;
            Name = _field.name;
            Value = _field.Value;
            Location = new TradeDataLocation(Int32.Parse(_field.b), Int32.Parse(_field.l), Int32.Parse(_field.r), Int32.Parse(_field.t));
 
        }
        public void  SetValue(string value)
        {
            _field.Value = value;

        }
        public string Name { get; set; }
        public string Value { get; set; }
        public TradeDataLocation Location { get; set; }
        public string PageNumber { get; set; }
        public override string ToString()
        {
            return _field.Value;
        }
    }
}
