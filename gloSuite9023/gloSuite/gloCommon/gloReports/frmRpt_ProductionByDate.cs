using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;


namespace gloReports
{
    public partial class frmRpt_ProductionByDate : Form
    {

        #region " Declarations "

            //For Creating the object of the Report Viewer User Control
            gloReportViewer _ogloReportViewer;

            //For Creating the Object of the CrystalReport
            Rpt_ProductionByDate objrptProductionByDate;

            private string _databaseconnectionstring = "";
            private string _MessageBoxCaption = string.Empty;
            System.Collections.Specialized.NameValueCollection appSettings = System.Configuration.ConfigurationManager.AppSettings;
            private Int64 _ClinicID = 0;
        
        #endregion " Declarations "
        

        #region  " Property Procedures "

            public Int64 ClinicID
            {
                get { return _ClinicID; }
                set { _ClinicID = value; }
            }

        #endregion  " Property Procedures "
        

        #region " Constructor "

            public frmRpt_ProductionByDate(string databaseconnectionstring)
            {
                InitializeComponent();

                _ogloReportViewer = new gloReportViewer();

                //Attaching the event Handler
                _ogloReportViewer.onReportsClose_Clicked += new gloReportViewer.onReportsCloseClicked(ogloReports_onReportsClose_Clicked);
                _ogloReportViewer.onGenerateReport_Clicked += new gloReportViewer.onGenerateReportClicked(ogloReports_onGenerateReport_Clicked);

                _databaseconnectionstring = databaseconnectionstring;
                if (appSettings["ClinicID"] != null)
                {
                    if (appSettings["ClinicID"] != "")
                    { _ClinicID = Convert.ToInt64(appSettings["ClinicID"]); }
                    else { _ClinicID = 0; }
                }
                else
                { _ClinicID = 0; }
            }

        #endregion
        

        #region "Form Events"

        private void frmRpt_ProductionByDate_Load(object sender, EventArgs e)
        {
            //For Addding the ReportViewer User Control in form
            pnlContainer.Controls.Add(_ogloReportViewer);
            _ogloReportViewer.Dock = DockStyle.Fill;

            //For Hiding the controls from the Search Criteria
            _ogloReportViewer.showTransCriteria = true;
            _ogloReportViewer.showDatesCriteria = true;

            _ogloReportViewer.setdatesAsCurrentMonth();

            //For Selecting Charges or Allowed amount
            _ogloReportViewer.showAmountType =false ;

            //Property to show the Export Button on Tool Bar
            _ogloReportViewer.showExport = true;
 

            FillProductionByDate(gloDateMaster.gloDate.DateAsNumber(_ogloReportViewer.dtStartDate.ToShortDateString()), gloDateMaster.gloDate.DateAsNumber(_ogloReportViewer.dtEndDate.ToShortDateString()));

        }

        #endregion


        #region "Fill Methods"

        private void FillProductionByDate(Int32 nFromDate, Int32 nToDate)
        {

            objrptProductionByDate = new Rpt_ProductionByDate();
            dsReports dsReports = new dsReports();
            SqlCommand _sqlcommand = new SqlCommand();
            SqlConnection oConnection = new SqlConnection();
            try
            {
                oConnection.ConnectionString = _databaseconnectionstring;
                _sqlcommand.CommandType = CommandType.StoredProcedure;
                _sqlcommand.CommandText = "Rpt_Production_By_Date";
                _sqlcommand.Connection = oConnection;
                _sqlcommand.Parameters.Add("@nFromDate", System.Data.SqlDbType.Int);
                _sqlcommand.Parameters.Add("@nToDate", System.Data.SqlDbType.Int);

                _sqlcommand.Parameters["@nFromDate"].Value = nFromDate;
                _sqlcommand.Parameters["@nToDate"].Value = nToDate;

                #region "Show Hide The Charges And Allowed Colums"

                //If Allowed Amount Pass the Parameter as 1
                if (_ogloReportViewer.bAllowed)
                {
                    _sqlcommand.Parameters.Add("@Mode", System.Data.SqlDbType.Int);
                    _sqlcommand.Parameters["@Mode"].Value = 1;
                }

                #endregion

                SqlDataAdapter da = new SqlDataAdapter(_sqlcommand);
                da.Fill(dsReports, "dt_ProductionByDate");
                da.Dispose();

                objrptProductionByDate.SetDataSource(dsReports);
                
                //Binds the Report to the Report viewer
                _ogloReportViewer.ReportViewer = objrptProductionByDate;
               

            }
           catch (gloDatabaseLayer.DBException ex)
            {
                ex.ERROR_Log(ex.ToString());
                gloAuditTrail.gloAuditTrail.ExceptionLog(ex.ToString(), true);
            }
            catch (Exception ex)
            {
                gloAuditTrail.gloAuditTrail.ExceptionLog(ex.ToString(), true);
            }
            finally
            {
                if (_sqlcommand != null)
                {
                    _sqlcommand.Parameters.Clear();
                    _sqlcommand.Dispose();
                    _sqlcommand = null;
                }
                if (oConnection != null && oConnection.State == ConnectionState.Open)
                {
                    oConnection.Close();
                    oConnection.Dispose();

                }
            }
        }

        #endregion


        #region "User Control Events"

        //Event Fot Closing the Form
        private void ogloReports_onReportsClose_Clicked(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ogloReports_onGenerateReport_Clicked(object sender, EventArgs e)
        {
            FillProductionByDate(gloDateMaster.gloDate.DateAsNumber(_ogloReportViewer.dtStartDate.ToShortDateString()), gloDateMaster.gloDate.DateAsNumber(_ogloReportViewer.dtEndDate.ToShortDateString()));
        }

        #endregion

    }
}