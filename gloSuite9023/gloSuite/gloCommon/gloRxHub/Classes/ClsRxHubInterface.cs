using System;
using System.Text;
using System.Xml;
using Edidev.FrameworkEDI;
using System.IO;
using System.Windows.Forms;
using System.Data;
using System.Collections;
using gloRxHub.WCFRxElignMedHx;
using System.ServiceModel;
using gloEMRGeneralLibrary.gloEMRPrescription;
using System.Data.SqlClient;
using System.Text.RegularExpressions;


namespace gloRxHub
{
    public class ClsRxHubInterface : IDisposable 
    {
        public  string Consent = "";
        public bool _MoreHistoryAvailableFlag ;
        public DateTime sStartDate;
        public DateTime sEndDate;
        private ClsPatient oPatient = null;
        public string strRxHisReqPath  ="";
        private clsgloRxHubDBLayer oclsgloRxHubDBLayer;
            // IDisposable
        private bool disposedValue = false;
        public string EDIString = "";

        public gloRxHub.ClsPatient Patient
        {
            get { return oPatient; }
            set { oPatient = value; }
        }

        /// <summary>
        /// this property procedure was especially created to show the Last Eligibility Request Datetime in message box, when the eligibility for patient is expired and user tries to get medication history
        /// code implemented to resolve residual bug # 42438 added in 7020 
        /// </summary>
        public string _dtLastEligibilityRequestDatetime = string.Empty;
        public string dtLastEligibilityRequestDatetime
        {
            get
            {
                return _dtLastEligibilityRequestDatetime;
            }
            set
            {
                _dtLastEligibilityRequestDatetime = value;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                }
                // TODO: free managed resources when explicitly called 
                //SLR: FRee oPatient
                if (oPatient != null)
                {
                    oPatient.Dispose();
                    oPatient = null;
                }
            }

            // TODO: free shared unmanaged resources 
            this.disposedValue = true;
        }

        #region " IDisposable Support "
        // This code added by Visual Basic to correctly implement the disposable pattern. 
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(ByVal disposing As Boolean) above. 
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion 


        #region " Constructors "

        public ClsRxHubInterface()
        {
            oPatient = new ClsPatient();
        }

        public ClsRxHubInterface(string DatabaseConnectionString)
        {
            _databaseconnectionstring = DatabaseConnectionString;
         }
        public ClsRxHubInterface(Int64 PatientId)
        {
            _PatientId = PatientId;
        }
        #endregion " Constructors "

        #region " Public And Private variables "

        string _databaseconnectionstring = "";
        string _messageboxcaption = "gloEMR";
        string sPath = "";
        string sSEFFile = "";
        string sEdiFile = "";
        public string EDIReturnResult = "";
      
        //ediWarnings oWarnings = null;
        //ediWarning oWarning = null;
        Int64 _PatientId = 0;
        #endregion " Public And Private variables "

        #region "ANSI 5010 Functions"

        public string Generate270_5010(string RxhubParticipantId, string RxHubPassword)
        {
            ClsgloRxHubGeneral.UpdateLog("Start Generate270_5010 ");
            gloRxHub.clsgloRxHubDBLayer oclsgloRxHubDBLayer = new gloRxHub.clsgloRxHubDBLayer(); ;
            //string _270FilePath = "";
            string BHT_TransactionRef = "";
            ediDocument oEdiDoc = null;
            ediInterchange oInterchange = null;
           ediGroup oGroup = null;
            ediTransactionSet oTransactionset = null;
            ediDataSegment oSegmentISA = null;
            ediDataSegment oSegmentGS = null;
            ediDataSegment oSegmentST = null;
            ediDataSegment oSegmentBHT = null;
            ediDataSegment oSegmentHL = null;
            ediDataSegment oSegmentNM1 = null;
            ediDataSegment oSegmentHL2 = null;
            ediDataSegment oSegmentNM2 = null;
            ediDataSegment oSegmentREF = null;
            ediDataSegment oSegmentN3 = null;
            ediDataSegment oSegmentN4 = null;
            ediDataSegment oSegmentSubscriberHL = null;
            ediDataSegment oSegmentSubscriberNM1 = null;
            ediDataSegment oSegmentSubscriberREF = null;
            ediDataSegment oSegmentSubscriberN3 = null;
            ediDataSegment oSegmentSubscriberN4 = null;
            ediDataSegment oSegmentSubscriberDMG = null;
            ediDataSegment oSegmentSubscriberDT = null;
            ediDataSegment oSegmentEQ = null;
           
            //ediSchema oSchema = null;
            //ediSchemas oSchemas = null;
            //ediAcknowledgment oAck = null;
            //fill the subscriber object information
            gloRxHub.ClsSubscriber oSubscriber ; //SLR: no new is neededed since it is a reference of patient.subscriber[i]
            try
            {
                ClsgloRxHubGeneral.UpdateLog("Start Try Block ");
                             
                ClsgloRxHubGeneral.UpdateLog("Create EDI object ");
           //     oEdiDoc = new ediDocument();
                ediDocument.Set(ref oEdiDoc, new ediDocument());
                if (oEdiDoc == null)
                {
                    oEdiDoc = new ediDocument();
                    ClsgloRxHubGeneral.UpdateLog("Created EDI object ");
                }
               
                //oEdiDoc.New();
                oEdiDoc.CursorType = DocumentCursorTypeConstants.Cursor_ForwardWrite;
                oEdiDoc.set_Property(DocumentPropertyIDConstants.Property_DocumentBufferIO, 2000);

                oEdiDoc.SegmentTerminator = "~";//"~\r\n"
                oEdiDoc.ElementTerminator = "*";
                oEdiDoc.CompositeTerminator = ":";//">"
                ClsgloRxHubGeneral.UpdateLog("Create ISA segment ");
                //ISA: Create the Interchange Control Header segment
                //Purpose: To start and identify an interchange of zero or more functional groups and interchange-related control segments
                //Example ANSI 4010: ISA*00*          *01*FXTXGJVZ0W*ZZ*T00000000020315*ZZ*RXHUB          *110630*1210*U*00401*068011012*1*T*:~
                //Example ANSI 5010: ISA*00*          *01*PWPHY12345*ZZ*POCID*ZZ*S00000000000001*110630*1210*^*00501*068011012*1*T*:~
                #region "ISA - Interchange Control Header"
               
                //ANSI 5010: instead of "004010" replace with "005010"
               
                ediInterchange.Set(ref oInterchange, (ediInterchange)oEdiDoc.CreateInterchange("X", "005010"));

                ClsgloRxHubGeneral.UpdateLog("Create Interchange segment ");
               
                if (oInterchange == null)
                {
                    oInterchange = new ediInterchange();
                    ClsgloRxHubGeneral.UpdateLog("Created Interchange segment ");
                }
              
                ediDataSegment.Set(ref oSegmentISA, (ediDataSegment)oInterchange.GetDataSegmentHeader());

                if (oSegmentISA == null)
                {
                    oSegmentISA = new ediDataSegment();
                    ClsgloRxHubGeneral.UpdateLog("Created Datasegment segment ");
                }
                //authorization Information Qualifier
                //Code to identify the type of information in the Authorization Information
                //00        No Authorization Information Present (No Meaningful Information in 102)
                oSegmentISA.set_DataElementValue(1, 0, "00");


                //Authorization Information - this is a blank value when we create the 270 file
                //Infomiation used for additional identification or authonzaon of the interchange sender or the data in the interchange; the type of information is
                //set by the Authorization Information Qualifier (lO1)      *Blank


                //Security Information Qualifier
                //Code to identit� the type of infomation in the Security Information 
                //01        Password
                oSegmentISA.set_DataElementValue(3, 0, "01");


                //depending on the Staging or Production pass the password
                //Security Information 
                //This is used for identifying the security information about the interchange sender or the data in me interchange; the type of information is set by the
                //Security Infomiation Qualifier (103)
                //From the POC/PPMS, this is the Password assigned by Surescnpts for the POCIPPMS.
                //From Surescripts, this is the password for Surescripts to get to the PBM/Payer.
                oSegmentISA.set_DataElementValue(4, 0, RxHubPassword);//"FXTXGJVZ0W"

                //Interchange ID Qualifier
                //Qualifier to designate the system/method of code structure used to designate the sender or receiver ID element being qualifier
                //ZZ        Mutually Defined
                oSegmentISA.set_DataElementValue(5, 0, "ZZ");

                //pass either Staging or production participant ID
                //Interchange Sender ID
                //Identification code published by the sender for other parties to use as the receiver ID to route data to them; the sender always codes this value in the sender ID element
                //From the POC/PPMS this is the POC�PPMS participant ID as assigned by Surescripts.
                //From Surescr�pts, this �s Surescripts� participant ID
                oSegmentISA.set_DataElementValue(6, 0, RxhubParticipantId);//"T00000000020315"

                //Interchange ID Qualifier
                //Qualifier to designate the system/method of code structure used to designate the sender or receiver ID element being qualified
                //ZZ        Mutually Defined
                oSegmentISA.set_DataElementValue(7, 0, "ZZ");

                //From POC/PPMS this is the RxHub's participant ID as assigned by RxHub
                //From RxHub, this is the PBM's participant ID

                //ANSI 5010: Interchange Receiver ID
                //Identification code published by the receiver of the data: When sending, it is used by the sender as their sending ID, thus other parties sending to them
                //will use this as a receiving ID to route data to them. From the POC/PPMS this is Surescflpts participant ID as assigned by Surescripts. From Surescnpts, this is PBM�s participant ID.
                oSegmentISA.set_DataElementValue(8, 0, "S00000000000001");//"RxHub"

                //Interchange Date 
                //Date of the interchange *Date format YYMMDD required
                string ISA_Date = Convert.ToString(gloDateMaster.gloDate.DateAsNumber(DateTime.Now.ToShortDateString()));
                oSegmentISA.set_DataElementValue(9, 0, gloDateMaster.gloDate.DateAsNumber(DateTime.Now.ToShortDateString()).ToString().Substring(2));//txtEnquiryDate.Text.Trim());//"010821");

                //Interchange Time 
                //Time of the interchange. *Time format HHMM required.
                string ISA_Time = Convert.ToString(gloDateMaster.gloTime.TimeAsNumber(DateTime.Now.ToLocalTime().ToShortTimeString()));
                oSegmentISA.set_DataElementValue(10, 0, FormattedTime(ISA_Time).Trim());

                //ANSI 5010: Repetition Separator
                //Type is not applicable; the repetition separator is a delimiter and not a data element; this field provides the delimiter used to separate repeated occurrences of
                //a simple data element or a compose data structure; this value must be different than me data element separator, component element separator, and the segment terminator.
                //Surescnpts recommends using Hex 1 F.
                oSegmentISA.set_DataElementValue(11, 0, "^");//"U" was used

                //ANSI 5010: Interchange Control Version Number
                //This version number covers the interchange control segments 
                //00501      Draft Standards for Trial Use Approved for Publication by ASC X12 Procedures Review Board through October 2003
                oSegmentISA.set_DataElementValue(12, 0, "00501");//"00401" was used


                //Interchange Control Number 
                //A control number assigned by the interchange sender From the POC!PPMS, this is a unique ID assigned by the POCIPPMS for transaction tracking.
                //From Surescripts, this is a unique ID assigned by Surescnpts for transaction trac)ing.
                //This ID will be returned on a TAl �f an error occurs. Providing a unique number will assist in resolving errors and tracking messages.
                string strControlNo = DateTime.Now.Millisecond.ToString("#000") + DateTime.Now.Second.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Hour.ToString(); //Convert.ToString(gloDateMaster.gloDate.DateAsNumber(DateTime.Now.ToShortDateString())) + Convert.ToString(gloDateMaster.gloTime.TimeAsNumber(DateTime.Now.ToLocalTime().ToShortTimeString())).Trim();
                oSegmentISA.set_DataElementValue(13, 0, strControlNo);

                //Acknowledgment Requested
                //Code sent by the sender to reouest an ntecran�e acknc�wledgrnent TA1) The TAl is returned only in the event of an error
                //TAl segments should not be returned for accepted transactions If there are no errors at the envelope level (ISA, CS, GE, lEA segments) then TAl segments should not be retumed
                //Since these transactions are real time only, Surescnpts does not use this field to detemiine wflether to create a TAl acknowledgement
                //0          No Acknowledgment Requested (Recommended by Suresc�pts)
                //1          Interchange Aocnowiedgrnent Requested
                oSegmentISA.set_DataElementValue(14, 0, "1");


                //please change this value according to Stating or Production. if it is pointing to staging then Pass "T" or else if it is pointing to Production then pass "P"
                //Usage Indicator 
                //Code to indicate whether data enclosed by this interchange envelope is test, production or infom�at�on
                //P         Production Data
                //T         Test Data
                if (RxhubParticipantId.StartsWith("T"))
                {
                    oSegmentISA.set_DataElementValue(15, 0, "T");
                }
                else
                {
                    oSegmentISA.set_DataElementValue(15, 0, "P");
                }

                //Component Element Separator 
                //Type is not applicable; the component element separator is a delimiter and not a data element: this field provides the delimiter used to separate
                //component data elements with�n a composite data structure; this value must be different than the data element separator and the segment terminator
                //*Surescripts recommends using Hex 1C.
                oSegmentISA.set_DataElementValue(16, 0, ":");

                #endregion "ISA - Interchange Control Header"

                //GS: Create the Functional Group Header Segment
                //Purpose: To indicate the beginning of a functional group and to provide control intomiation
                //Example ANSI 4010: GS*HS*T00000000020315*RXHUB*20110630*1210*1*X*004010X092A1~
                //Example ANSI 5010: GS*HS*POCID*S00000000000001*20110630*1210*1*X*005010X279~
                ClsgloRxHubGeneral.UpdateLog("Create GS segment ");
                #region "GS - Functional Group Header "
             

                ediGroup.Set(ref oGroup, (ediGroup)oInterchange.CreateGroup("005010X279A1"));//005010X279"004010X092A1" was used005010X279A1
                
                if (oGroup == null)
                {
                   
                        oGroup = new ediGroup();
                        ClsgloRxHubGeneral.UpdateLog("Created GS segment ");
                }

               
                ediDataSegment.Set(ref oSegmentGS, (ediDataSegment)oGroup.GetDataSegmentHeader());
                if (oSegmentGS == null)
                {
                    oSegmentGS = new ediDataSegment();
                    ClsgloRxHubGeneral.UpdateLog("Created Datasegment segment ");
                }
                //Functional Identifier Code 
                //Code idenntying a group of application related transaction sets 
                //HS        Eligibility, Coverage or Benefit Inquiry (270)
                oSegmentGS.set_DataElementValue(1, 0, "HS");


                //pass either Staging or production participant ID
                //Application Senders Code
                //Code identifying party sending transmission; codes agreed to by trading partners
                //*From the POCIPPMS this is the POC!PPMS participant ID as assigned by Surescripts. �From Surescripts, this is Surescripts participant ID.
                oSegmentGS.set_DataElementValue(2, 0, RxhubParticipantId);//"T00000000020315"


                //Application Receiver�s Code 
                //Code identifying party receiving transmission; codes agreed to by trading partners
                //*From the POC/PPMS this �s Surescripts� participant ID as assigned by Surescripts.
                //*From Surescripts, this is PBM�s participant ID.
                oSegmentGS.set_DataElementValue(3, 0, "S00000000000001");//Receiver ID

                //Date
                //Date expressed as CCYYMMDD
                oSegmentGS.set_DataElementValue(4, 0, Convert.ToString(gloDateMaster.gloDate.DateAsNumber(DateTime.Now.ToShortDateString())).Trim());

                //Time
                //Time expressed in 24-hour clock time as follows HHMM. or HHMMSS, or HHMMSSD, or HHMMSSDO, where H = hours (00-23), M = minutes (00-59), 
                //S integer seconds (00-59) and DO = decimal seconds; decimal seconds are expressed as follows: D = tenths (0-9) and OD = hundredths (00-99)
                string GS_Time = Convert.ToString(gloDateMaster.gloTime.TimeAsNumber(DateTime.Now.ToLocalTime().ToShortTimeString())).Trim();
                oSegmentGS.set_DataElementValue(5, 0, FormattedTime(GS_Time).Trim());

                //Group Control Number
                //Assigned number originated and maintained by the sender
                //The control number should be unique across all groups within this transaction set. This ID will be returned on an AK1O2 of the 999
                //acknowledgement �f an error occurs. Providing unique numbers will assist in resolving errors and tracking meSsages.
                oSegmentGS.set_DataElementValue(6, 0, "1");//H10014:Leading zeros detected in GS06. The X12 syntax requires the suppression of leading zeros for numeric elements

                //Responsible Agency Code 
                //Code used in conjunction with Data Element 480 to identify the issuer of the standard
                //X         Accredited Standards Committee X12
                oSegmentGS.set_DataElementValue(7, 0, "X");

                //Version Release Industry Identifier Code
                //Code indicating the version, release, subrelease, and industry identifier of the EDI standard being used, including the GS and GE segments; if code in DE455
                //in GS segment is X, then in DE 480 positions 1-3 are the version number; positions 4-6 are the release and subrelease, level of the version; and positions 7-12 are the industry o- trade association identifiers (optionally assigned by
                //user); if code in DE455 in GS segment is T, then other fomats are allowed
                //005010X279           Draft Standards Approved for Publication by ASC X12 Procedures Review Board through October 2003, as published in this implementation guide.
                oSegmentGS.set_DataElementValue(8, 0, "005010X279A1");//005010X279"004010X092A1" was used005010X279A1

                #endregion "GS - Functional Group Header "

                //ST: Create the Transaction Set Header
                //Purpose: To indicate the start of a transaction set and to assiQn a control number
                //Example ANSI 4010: ST*270*000000001~
                //Example ANSI 5010: ST*270*0001*005010X279~
                ClsgloRxHubGeneral.UpdateLog("Create ST segment ");
                #region "ST - Transaction Set Header"

                //Transaction Set Identifier Code
                //Code uniquely identifying a Transaction Set
                //Use this code to identify the transaction set ID for the transaction set tht w�ll follow the ST segment. Each X12 standard has a transaction set
                //identifier code that is unique to that transaction set
                //270       El�gibihty, Coverage or Benefit Inquiry
                try
                {
                    ediTransactionSet.Set(ref oTransactionset, (ediTransactionSet)oGroup.CreateTransactionSet("270"));

                    if (oTransactionset == null)
                    {
                        oTransactionset = new ediTransactionSet();
                        ClsgloRxHubGeneral.UpdateLog("Created ST segment ");
                    }
                }
                catch (Exception ex) 
                {
                    ClsgloRxHubGeneral.UpdateLog("not Created ST segment " + ex.ToString());
                }
                oSegmentST = null;
                
               
                //Implementation Convention Reference               
                //Reference assigned to identify Implementation Convention The implementation convention reference (STO3) is used by the translation
                //routines of the interchange partners to select the appropriate implementation convention to match the transaction set definition. When used, this
                //implementation convention reference takes precedence over the implementation reference specified in the GSO8 This element must be populated with 005010X279.
                //This element contains the same value as GSO& Some translator products strip off the ISA and GS segments prior to application (ST�SE) processing.
                //Providing me information from the GSO8 at this level will ensure that the appropriate application mapping is utilized at translation time
                ediDataSegment.Set(ref oSegmentST, (ediDataSegment)oTransactionset.GetDataSegmentHeader());
                if (oSegmentST == null)
                {
                    oSegmentST=new ediDataSegment();
                }
                //Transaction Set Control Number
                //Identifying control number that must be unique within the transaction set functional group assigned by the originator for a transaction set
                //The transaction set control numbers in STO2 and SEO2 must be identical. This unique number also aids in error resolution research Start with the
                //number, for example �0001�, and increment from there This number must be unique within a specific group and interchange, but can repeat in other groups and interchanges.
                //*This ID will be returned on an AK202 of the 999 acknowledgement if an error occurs. Providing a unique number will assist in resolving errors and tracking messages.
                oSegmentST.set_DataElementValue(2, 0, "000000001");


                //Implementation Convention Reference 
                //Reference assigned to identify Implementation Convention The implementation convention reference (STO3) is used by the translation routines of the interchange partners to select the appropriate implementation
                //convention to match the transaction set definition. When used, this implementation convention reference takes precedence over the implementation reference specified in the GSO8 This element must be populated with 005010X279.
                //This element contains the same value as GSO8 Some translator products strip off the ISA and GS segments prior to application (ST/SE) processing.Providing the information from the GSO8 at this level will ensure that the
                //appropriate application mapping is utilized at translation time
                oSegmentST.set_DataElementValue(3, 0, "005010X279A1");//005010X279 005010X279A1


                #endregion "ST - Transaction Set Header"

                //BHT Beginning of Hierarchical Transaction
                //Purpose: To define the business hierarchical structure of the transaction set and identify the business application purpose and reference data, ie, number, date, and time
                //Example ANSI 4010: BHT*0022*13*0630201112101696*20110630*1210~
                //Example ANSI 5010: BHT*0022*13*19980011400000*19980101*1400~
                ClsgloRxHubGeneral.UpdateLog("Create BHT segment ");
                #region "BHT - Beginning of Hierarchical Transaction"
                oSegmentBHT = null;
                //Begining Segment 
                ediDataSegment.Set(ref oSegmentBHT, (ediDataSegment)oTransactionset.CreateDataSegment("BHT"));
                 if (oSegmentBHT == null)
                {
                    oSegmentBHT=new ediDataSegment();
                }
                //Hierarchical Structure Code
                //Code indicating the hierarchical application structure of a transaction set that utilizes the HL segment to define the structure of the transaction set
                //Use this code to specify the sequence of hierarchical levels that may appear in the transaction set This code only indicates the sequence of the levels, 
                //not the requirement that all levels be present. For example, if code �0022� is used. the dependent level may or may not be present for each subscriber.
                //0022      Information Source, Infomiation Receiver, Subscriber,Dependent
                oSegmentBHT.set_DataElementValue(1, 0, "0022");

                //Transaction Set Purpose Code
                //Code identifying purpose of transaction set
                //13        Request 
                //Note: Surescripts Participants utilize this option only (Cancellation) and 36 (Authority to Deduct) are not utilized.
                oSegmentBHT.set_DataElementValue(2, 0, "13");

                //Reference Identification
                //Reference informaflon as defined for a particular Transaction Set or as specified by the Reference Identification Qualifier
                //Required when the transaction is processed in Real Time. If not required by this implementation guide, may be provided at the senders discretion, but cannot be required by the receiver.
                //This element is to be used to trace me transaction from one point to the next point, such as when the transaction is passed from one cleannghouse to another
                //clearinghouse. This identifier is to be re:umed in me corresponding 271 transaction�s BHTO3. This identifier will only be returned by the last entity to
                //handle the 270. This identifier will not be passed through the complete life of the transaction. All recipients of 270 transactions are required to return the Submitter Transaction Identifier in their 271 response if one is submitted. Submitter Transaction Iclentitier
                BHT_TransactionRef = DateTime.Now.Date.ToString("MMddyyyy") + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + DateTime.Now.Millisecond.ToString(); //Convert.ToString(gloDateMaster.gloDate.DateAsNumber(DateTime.Now.ToShortDateString())) + Convert.ToString(gloDateMaster.gloTime.TimeAsNumber(DateTime.Now.ToLocalTime().ToShortTimeString())).Trim();
                oSegmentBHT.set_DataElementValue(3, 0, BHT_TransactionRef);

                //Date
                //Date expressed as CCYYMMDD
                //Use this date for the date the transaction set was generated. Format CCYYMMDD
                oSegmentBHT.set_DataElementValue(4, 0, Convert.ToString(gloDateMaster.gloDate.DateAsNumber(DateTime.Now.ToShortDateString())));

                //Time
                //Time expressed �n 24-hour clock time as follows: HHMM, or HHMMSS, or HHMMSSD, or HHMMSSDD, where H = hours (00-23), M = minutes (00-59), 
                //S = integer seconds (00-59) and DD = decimal seconds: decimal seconds are expressed as follows: D = tenths (O-9) and DD = hundredths (00-99)
                //Use this time for the time the transaction set was generated. Time format HHMMSSDD required.
                string BHT_Time = Convert.ToString(gloDateMaster.gloTime.TimeAsNumber(DateTime.Now.ToLocalTime().ToShortTimeString())).Trim();
                oSegmentBHT.set_DataElementValue(5, 0, FormattedTime(BHT_Time).Trim());

                #endregion "BHT - Beginning of Hierarchical Transaction"

                ClsgloRxHubGeneral.UpdateLog("Create HL1 segment ");
                //Information Source Level
                //Purpose: To identify dependencies among and the content of hierarchically related groups of data segments
                //Example ANSI 4010: HL*1**20*1~
                //Example ANSI 5010: HL*1**20*1~
                #region "HL - Information Source Level"
                oSegmentHL = null;

                ediDataSegment.Set(ref oSegmentHL, (ediDataSegment)oTransactionset.CreateDataSegment("HL\\HL"));
                if (oSegmentHL == null)
                {
                    oSegmentHL = new ediDataSegment();
                }
                //Hierarchical ID Number 
                //A unique number assigned by the sender to identify a particular data segment in a hierarchical structure
                //Use this sequentially assigned positive number to identify each specific occurrence of an HL segment within a transaction set. It should begin with the
                //number one and be incremented by one for each successive occurrence of the HL segment within that specific transaction set (ST through SE).
                //An example of the use of the HL segment and this data element is:
                //HL*1**20*1~
                //NM1*2B*2*RXHUB*****PI*RXHUB~
                oSegmentHL.set_DataElementValue(1, 0, "1");

                //Hierarchical Level Code 
                //Code defining the characteristic of a level in a hierarchical structure
                //All data that follows an HL segment is associated with the entity �dentified by the level code; this association continues until the next occurrence of an HL segment.
                //20        Information Source Identities the payer, maintainer, or source of the information
                oSegmentHL.set_DataElementValue(3, 0, "20");

                //Hierarchical Child Code 
                //Code indicating if there are hierarchical child data segments subordinate to the level being described
                //Use this code to indicate whether there are additional hierarchical levels subordinate to the current hierarchical level
                //Because of the hierarchical structure, and because an additional HL always exists in this transaction. the code value in the HLO4 at the Loop 2000A level should always be �1�.
                //1         Additional Subordinate HL Data Segment in This Hierarchical Structure.
                oSegmentHL.set_DataElementValue(4, 0, "1");

                #endregion "HL - Information Source Level"
                ClsgloRxHubGeneral.UpdateLog("Create NM1 segment ");
                //NM1: Information Source Name
                //Purpose: To supply the full name of an individual or organizational entity
                //Example ANSI 4010: NM1*2B*2*RXHUB*****PI*RXHUB~
                //Example ANSI 5010: NM1*2B*2*SURESCRIPTS LLC*****PI*RXHUB~
                #region "NM1 - Information Source Name"
                oSegmentNM1 = null;

                ediDataSegment.Set(ref oSegmentNM1, (ediDataSegment)oTransactionset.CreateDataSegment("HL\\NM1\\NM1"));
                if (oSegmentNM1 == null)
                {
                    oSegmentNM1 = new ediDataSegment();
                }
                //Entity Identifier Code 
                //Code identifying an organizational entity, a physical location, property or an individual
                //2B        Third-Party Administrator (Recommended by Surescnpts)
                oSegmentNM1.set_DataElementValue(1, 0, "2B");

                //Entity Type Qualifier 
                //Code qualifying the type of entity
                //Use this code to indicate whether the entity is an individual person or an organization
                //1         Person
                //2         NonPerson Entity (Recommended by Surescripts)
                oSegmentNM1.set_DataElementValue(2, 0, "2");//2=Non-Person Entity

                //Name Last or Organization Name 
                //Individual last name or organizational name
                //*From the POC/PPMS, the source is unknown so this would be Surescnpts.
                //*From Surescripts, Surescnpts will place the source name here.
                oSegmentNM1.set_DataElementValue(3, 0, "SURESCRIPTS LLC");

                //Identification Code Qualifier 
                //Code designating the system/method of code structure used for Identification Code (67)
                //PI        Payer Identification (Recommended by Surescnpts)
                oSegmentNM1.set_DataElementValue(8, 0, "PI");

                //Identification Code 
                //Code identifying a party or other code
                //Use this reference number as quaIifiedthe preceding data element (NM1O8).
                //*From the POC/PPMS, the PBM is unknown so this will be Surescripts� participant ID.
                //*From the Surescripts, Surescripts will place the participant ID of the PBM here.
                oSegmentNM1.set_DataElementValue(9, 0, "S00000000000001");

                #endregion "NM1 - Information Source Name"


                //HL: Information Receiver Level
                //Purpose: To identify dependencies among and the content of hierarchically related groups of data segments
                //* Physician identification
                //Example ANSI 4010: HL*2*1*21*1~
                //Example ANSI 5010: HL*2*1*21*1~
                ClsgloRxHubGeneral.UpdateLog("Create HL2 segment ");
                #region "HL - Information Receiver Level"
                oSegmentHL2 = null;
                ediDataSegment.Set(ref oSegmentHL2, (ediDataSegment)oTransactionset.CreateDataSegment("HL(2)\\HL"));
                if (oSegmentHL2 == null)
                {
                    oSegmentHL2 = new ediDataSegment();
                }
                //Hierarchical ID Number
                //A unique number assigned by the sender to identify a particular data segment in a h�erarchical structure
                oSegmentHL2.set_DataElementValue(1, 0, "2");

                //Hierarchical Parent ID Number 
                //Identification number of the next higher hierarchical data segment that the data segment being described is subordinate to
                oSegmentHL2.set_DataElementValue(2, 0, "1");

                //Hierarchical Level Code 
                //Code defining the characteristic of a level in a hierarchical structure
                //21        Information Receiver Identifies the provider or party(ies) who are the recipient(s) of the information
                oSegmentHL2.set_DataElementValue(3, 0, "21");

                //Hierarchical Child Code 
                //Code indicating if there are hierarchical child data segments subordinate to the level being described
                //1         Additional Subordinate HL Data Segment in This Hierarchical Structure.
                oSegmentHL2.set_DataElementValue(4, 0, "1");//1=Additional Subordinate HL Data segment in this Herarchical structure
                #endregion "HL - Information Receiver Level"

                ClsgloRxHubGeneral.UpdateLog("Create NM2 segment ");
                //NM1: Information Receiver Name
                //Purpose: To supply the full name of an individual or organizational entity
                //Example ANSI 4010: NM1*1P*1*Test*Provider 1*D***XX*5474745584~
                //Example ANSI 5010: NM1*1P*1*JONES*TIM*D***XX*111223333~
                #region "NM1 - Information Receiver Name"
                oSegmentNM2 = null;
                //INFORMATION RECEIVER NAME (It is the medical service Provider)
                ediDataSegment.Set(ref oSegmentNM2, (ediDataSegment)oTransactionset.CreateDataSegment("HL(2)\\NM1\\NM1"));
                if (oSegmentNM2 == null)
                {
                    oSegmentNM2 = new ediDataSegment();
                }
                //Entity Identifier Code 
                //Code identifying an organizational entity, a physical location, property or an individual
                //1P Provider (Recommended by Surescripts)
                oSegmentNM2.set_DataElementValue(1, 0, "1P");

                //Entity Type Qualifier 
                //Code qualifying the type of entity Use this code to indicate whether the entity is an �nd�vidual person or an organization
                //1         Person (Recommended by Surescnpts)
                //2         Non-Person Entity
                oSegmentNM2.set_DataElementValue(2, 0, "1");

                //Name Last or Organization Name 
                //Indvidual name or Organizational name
                //Use this name for the organizatiows name �f the entity type qualifier is a non person entity. Otherwise, use this name for the �ndividuals last name. 
                //Use if name infom�ation is needed to identify the receiver of eligibility or benefit information
                //*Physician Name
                oSegmentNM2.set_DataElementValue(3, 0, oPatient.Provider.ProviderLastName);//"JONES"

                //Name First 
                //Individual first name
                //Use this name only if NM1O2 is �1�.
                oSegmentNM2.set_DataElementValue(4, 0, oPatient.Provider.ProviderFirstName);//"TIM"

                //Name Middle 
                //Individual middle name or initial
                //Use this name only if NM102 is �1�.
                oSegmentNM2.set_DataElementValue(5, 0, oPatient.Provider.ProviderMiddleName);//"D"

                //prefix tag not shown in the Prescription Benefit IG 2011-04-15.pdf doc for ANSI 5010
                oSegmentNM2.set_DataElementValue(6, 0, "");

                //Name Suffix
                //Suffix to individual name
                oSegmentNM2.set_DataElementValue(7, 0, oPatient.Provider.ProviderSuffix);

                //Identification Code Qualifier 
                //Code designating the systenvmethod of code structure used for Identification Code (67)
                //See X12 Guide. Use this element to qualify the identification number submitted in NM1O9. This is the number that the information source associates with the information receiver.
                if (oPatient.Provider.ProviderNPI != "")//XX is used for identification therefore we will send the providers NPI first 
                {
                    //XX            Health Care Financing Administration National Provider Identifier
                    //              Required value if the National Provider ID is mandated for use. Otherwise, one of the other listed codes may be used.
                    //              ***The NPI is now mandated. Surescripts will only reject if the NM1O8 and the NM1O9 are not populated.
                    //              Surescnpts will not be validating the NPI. but some payers may validate it.
                    oSegmentNM2.set_DataElementValue(8, 0, "XX");

                    //Identification Code 
                    //Code identitying a party or other code Use this reference number as qualified by the preceding data element (NM 108).
                    oSegmentNM2.set_DataElementValue(9, 0, oPatient.Provider.ProviderNPI);
                }
                else//use providers DEA with SV as identification
                {
                    //oSegment.set_DataElementValue(8, 0, "SV");
                    oSegmentNM2.set_DataElementValue(8, 0, "XX");//changed from SV to XX for resolving Bug #57764: in 8000 : gloEMR > RxEligibility >when provider with only DEA sends eligibility request then it is not received

                    //Identification Code 
                    //Code identitying a party or other code Use this reference number as qualified by the preceding data element (NM 108).
                    oSegmentNM2.set_DataElementValue(9, 0, oPatient.Provider.ProviderDEA);
                }


                #endregion "NM1 - Information Receiver Name"

                ClsgloRxHubGeneral.UpdateLog("Create REF segment ");
                //REF: Information Receiver Additional Identification (POC Identification)
                //Purpose: To specify identifying information
                //Example ANSI 4010: REF*EO*T00000000020315~
                //Example ANSI 5010: REF*EO*POCID~
                #region "REF - Information Receiver Additional Identification (POC Identification/Physician System Identification)"

                ediDataSegment.Set(ref oSegmentREF, (ediDataSegment)oTransactionset.CreateDataSegment("HL(2)\\NM1\\REF"));
                if (oSegmentREF == null)
                {
                    oSegmentREF = new ediDataSegment();
                }
                //Reference Identification Qualifier 
                //Code qualifying the Reference Identification
                //Use this code to specify or qualify the type of reference number that is following in REFO2, REFO3, or both.
                //EO            Submitter Identification Number. 
                //              A unique number identifying the submitter of the transaction set
                //              *Surescripts defined participant ID for the POC/PPMS.
                oSegmentREF.set_DataElementValue(1, 0, "EO");

                //add the staging or production participant id
                //Reference Identification 
                //Reference information as defined for a particular Transaction Set or as specified the Reference Identification Qualifier
                //Use this reference number as qualified by the preceding data element (REF01).
                oSegmentREF.set_DataElementValue(2, 0, RxhubParticipantId);//"T00000000020315"

                #endregion "REF - Information Receiver Additional Identification (POC Identification/Physician System Identification)"

                ClsgloRxHubGeneral.UpdateLog("Create N3 segment ");
                //N3: Information Receiver Address
                //Purpose: To specify the location of me named party
                //Example ANSI 4010: N3*1 250 BMH Physician Building~
                //Example ANSI 5010: N3*55 HIGH STREET~
                #region "N3 - Information Receiver Address"

                ediDataSegment.Set(ref oSegmentN3, (ediDataSegment)oTransactionset.CreateDataSegment("HL(2)\\NM1\\N3"));
                if (oSegmentN3 == null)
                {
                    oSegmentN3 = new ediDataSegment();
                }
                //Address Information 
                //Address information
                //Use this infomiation for the first line of the address information.
                string ProviderAddres = "";
                ProviderAddres = oPatient.Provider.ProviderAddress.AddressLine1 + " " + oPatient.Provider.ProviderAddress.AddressLine2;
                oSegmentN3.set_DataElementValue(1, 0, ProviderAddres.Trim());

                #endregion "N3 - Information Receiver Address"

                ClsgloRxHubGeneral.UpdateLog("Create N4 segment ");
                //N4: Information Receiver Citi/State/ZIP Code
                //Purpose: To specify the geographic place of the named party
                //Example ANSI 4010: N4*Maryville*TN*37804~
                //Example ANSI 5010: N4*SEATTLE*WA*98123*US~
                #region"N4 - Information Receiver Citi/State/ZIP Code"

                ediDataSegment.Set(ref oSegmentN4, (ediDataSegment)oTransactionset.CreateDataSegment("HL(2)\\NM1\\N4"));
                if (oSegmentN4 == null)
                {
                    oSegmentN4 = new ediDataSegment();
                }
                //City Name 
                //Free-form text for city name
                //Use this text for the city name of the information receiver�s address.
                oSegmentN4.set_DataElementValue(1, 0, oPatient.Provider.ProviderAddress.City);

                //State or Province Code 
                //Code (Standard State/Province) as defined by appropriate government agency Required when the address is in the United States of America. 
                //including its territories, or Canada. If not required by this implementation guide, do not send.
                oSegmentN4.set_DataElementValue(2, 0, oPatient.Provider.ProviderAddress.State);

                //Postal Code 
                //Code defining international postal zone code excluding punctuation and blanks (zip code for United States)
                //required when the address is in the United States of America. including its teritories, or Canada, or when a postal code exists for the country in N404. 
                //If not required by this implementation guide, do not send.
                if (oPatient.Provider.ProviderAddress.Zip != "" && oPatient.Provider.ProviderAddress.Zip.Length > 5)
                {
                    oPatient.Provider.ProviderAddress.Zip = oPatient.Provider.ProviderAddress.Zip.Substring(0, 5);
                }
                oSegmentN4.set_DataElementValue(3, 0, oPatient.Provider.ProviderAddress.Zip);

                //we dont send this tag
                //Country Code - Optional
                //Code identifying the country Use this code to specify the country of the information receiver�s address, if other than the United States 
                //*Do not send the US County Code


                #endregion"N4 - Information Receiver Citi/State/ZIP Code"

                ClsgloRxHubGeneral.UpdateLog("Create HL segment ");
                //HL: Subscriber Level
                //Purpose: To identify dependencies among and the content of hierarchically related groups of data segments
                //Example ANSI 4010: HL*3*2*22*0~
                //Example ANSI 5010: HL*3*2*22*0~
                #region"HL - Subscriber Level"
                ediDataSegment.Set(ref oSegmentSubscriberHL, (ediDataSegment)oTransactionset.CreateDataSegment("HL(3)\\HL"));
                if (oSegmentSubscriberHL == null)
                {
                    oSegmentSubscriberHL = new ediDataSegment();
                }
                //Hierarchical ID Number 
                //A unique number assigned by the sender to identify a particular data segment in a hierarchical structure
                oSegmentSubscriberHL.set_DataElementValue(1, 0, "3");

                //Hierarchical Parent ID Number 
                //Identification number of the next higher hierarchical data segment that the data segment being described is subordinate to
                oSegmentSubscriberHL.set_DataElementValue(2, 0, "2");

                //Hierarchical Level Code 
                //Code defining the characteristic of a level in a hierarchical structure 
                //22            Subscriber Identifies the employee or group member who is covered for insurance and to whom, or on behalf of whom. the insurer agrees to pay benefits
                oSegmentSubscriberHL.set_DataElementValue(3, 0, "22");

                //Hierarchical Child Code 
                //Code indicating if mere are hierarchical child data segments subordinate to the level being described
                //0             No Subordinate HL Segment in This Hierarchical Structure.
                //1             Additional Subordinate HL Data Segment in This Hierarchical Structure.
                oSegmentSubscriberHL.set_DataElementValue(4, 0, "0");


                #endregion "HL - Subscriber Level"


                //NM1: Subscriber Name
                //Purpose: To supply the full name of an individual or organizational en�ty
                //Example ANSI 4010: NM1*IL*1*CROSS*DAVID*M***ZZ*000000000~
                //Example ANSI 5010: NM1*IL*1*SMITH*ROBERT*B***MI*33399999~   //ex from "Prescription benefit IG 2011-04-15.pdf" document
                #region "NM1 - Subscriber Name"

                //SUBSCRIBER NAME(A person who can be uniquely identified to an information source. Traditionally referred to as a member.)
                ediDataSegment.Set(ref oSegmentSubscriberNM1, (ediDataSegment)oTransactionset.CreateDataSegment("HL(3)\\NM1\\NM1"));
                if (oSegmentSubscriberNM1 == null)
                {
                    oSegmentSubscriberNM1 = new ediDataSegment();
                }
                //Entity Identifier Code 
                //Code iden�tying an organizational entity, a physical location, property or an individual
                //IL            Insured or Subscriber
                oSegmentSubscriberNM1.set_DataElementValue(1, 0, "IL");

                //Entity Type Qualifier 
                //Code qualifying the type of entity Use this code to indicate whether the enty is an individual person or anorganization.
                //1             Person
                oSegmentSubscriberNM1.set_DataElementValue(2, 0, "1");


                if (oPatient.Subscriber.Count > 0)
                {
                    for (int i = 0; i < oPatient.Subscriber.Count; i++)
                    {
                        if (oPatient.Subscriber[i].SubscriberFirstName != "")
                        {
                            oSubscriber = oPatient.Subscriber[i];
                            break;
                        }

                    }
                    //oSubscriber = oPatient.Subscriber[i]; 
                }
              
                //Name Last or Organization Name 
                //Individual last name or organizational name Use this name for the subscnbers last name
                //Use this name �f the subscriber is the paflent and if utilizing the HIPAA search option. See Section 1.4.8 Search Options of the X12 Implementation Guide for more information.
                //oSegment.set_DataElementValue(3, 0, oSubscriber.SubscriberLastName);//"ThisOneLastnameisDangerWillRobinsonSommerfieldKrykowskiSmith"
                oSegmentSubscriberNM1.set_DataElementValue(3, 0, ReplaceSpecialChar(oPatient.LastName));//"PALTROW"

                //Name First - Individual first name
                //Use this name for the subscnbers first name. Use this name if the subscnber is the patient and �f utilizing the HIPAA search option. See Section 1.4.8 Search Options of the X12 Imolernentation Guide for more information.
                //oSegment.set_DataElementValue(4, 0, oSubscriber.SubscriberFirstName);//"FirstnameisPhilAlexandriaAnnapoliss"
                oSegmentSubscriberNM1.set_DataElementValue(4, 0, ReplaceSpecialChar(oPatient.FirstName));//"BRUCE"

                //Name Middle - Individual middle name or initial
                //Use this name for the subscribers middle name or initial. Use if infomiat�on is known and will assist in identification of the person named, particularly when not utilizing the HIPAA search option. 
                //oSegment.set_DataElementValue(5, 0, oSubscriber.SubscriberMiddleName);//"MiddlenamisJackAlexandria"
                oSegmentSubscriberNM1.set_DataElementValue(5, 0, ReplaceSpecialChar(oPatient.MiddleName));


                //Suffix
                //oSegment.set_DataElementValue(7, 0, "Junior iii");
                //Identification Code Qualifier 
                //Code designating the system/method of code structure used for Identification Code (67)
                //Use this element to qualify the identification number submitted in NM1O9. This is the primary number that the information source associates with the subscriber.
                //Use this element if utilizing the HIPAA search option. See Section 14.8 Search Options) of the X12 Implementation Guide for more information. 
                //*From the POC/PPMS this is blank. Surescripts will put the PBM Unique ID into this field.
                //MI            Member Identification Number
                //              This code may only be used prior to the mandated use of code �1 l. This is the unique number the payer or information source uses to identify the insured 
                //              (e.g., Health Insurance Claim Number, Medicaid Recipient ID Number, HM3 Member ID, etc.).
                oSegmentSubscriberNM1.set_DataElementValue(8, 0, "");//ZZ was used

                //Identification Code
                //Code idenbfying a party or other code
                //Subscriber Identification Code if available Frorn the POC/PPMS this is blank. Surescripts will put the PBM Unique ID into this field.
                if (oPatient.SSN.Trim() != "")
                {
                    //this was discussed with yaw on 12 march 09 Thursday and he asked to add the patient ssn no. previously we were passing subscriberId.
                    oSegmentSubscriberNM1.set_DataElementValue(9, 0, "");//oPatient.SSN was used
                }

                #endregion "NM1 - Subscriber Name"


                //REF: Subscriber Additional Identification - OPTIONAL
                //Purpose: To spec�y identifying information
                //Example ANSI 4010: REF*SY*000000000~
                //Example ANSI 5010: absent in "Prescription benefit IG 2011-04-15.pdf" document
                //Note: Original code is commented since in the document the segement is absent and also this is an OPTIONAL segment
                #region "REF - Subscriber Additional Identification"
                //integrated from gloSuite8000_POC, for Patient Saving Project requirements on 10 Oct 2013 Reopened the comment inorder to send the patient id in 270 Rxeligibility file 
                if (oPatient.PatientID != 0)
                {
                    ediDataSegment.Set(ref oSegmentSubscriberREF, (ediDataSegment)oTransactionset.CreateDataSegment("HL(3)\\NM1\\REF"));
                    if (oSegmentSubscriberREF == null)
                    {
                        oSegmentSubscriberREF = new ediDataSegment();
                    }
                    //Reference Identification Qualifier 
                    //Code qualifying the Reference Identification
                    //Use this code to specify or qualify the type of reference number that is following in REFO2, REFO3, or both. See X12 guide for additional qualifiers.
                    //SY               Social Security Number
                    //                 The social security number may not be used for any
                    //                 Federally administered programs such as Medicare.
                    //EJ               Patient Account Number
                    oSegmentSubscriberREF.set_DataElementValue(1, 0, "EJ");

                    //Reference Identification 
                    //Reference informat�on as def�ned for a particular Transaction Set or as specified by me Reference Identification Qualifier
                    oSegmentSubscriberREF.set_DataElementValue(2, 0, oPatient.PatientID.ToString());

                }

                #endregion "REF - Subscriber Additional Identification"

                //N3: Subscriber Address
                //Purpose: To specify the location of the named party
                //Example ANSI 4010: N3*6785 LAUGHALOT LANE ~
                //Example ANSI 5010: N3*29 FREMONT ST*APT#1 ~
                #region "N3 - Subscriber Address"

                ediDataSegment.Set(ref oSegmentSubscriberN3, (ediDataSegment)oTransactionset.CreateDataSegment("HL(3)\\NM1\\N3"));
                if (oSegmentSubscriberN3 == null)
                {
                    oSegmentSubscriberN3 = new ediDataSegment();
                }

                //string SubscriberAddressLine1n2 = "";
                //SubscriberAddressLine1n2 = oSubscriber.SubscriberAddress.AddressLine1;//"Patient address line 00001 goes into this location here"
                //Address Information 
                //Address information Line1
                //Use this information for the first line of the address information.
                //oSegment.set_DataElementValue(1, 0, SubscriberAddressLine1n2);

                string PatAddresln1 = "";
                PatAddresln1 = ReplaceSpecialChar(oPatient.PatientAddress.AddressLine1);

                oSegmentSubscriberN3.set_DataElementValue(1, 0, PatAddresln1);//"2645 MULBERRY LANE"//"Subscriber Address");

                //Address Information 
                //Address information Line1
                //Use this information for the second line of the address information
                //Required if a second address line exists.
                //SubscriberAddressLine1n2 = oSubscriber.SubscriberAddress.AddressLine2;//"Patient address line 00002 goes into this location here"
                //oSegment.set_DataElementValue(2, 0, SubscriberAddressLine1n2);

                string PatAddresln2 = "";
                PatAddresln2 = ReplaceSpecialChar(oPatient.PatientAddress.AddressLine2);

                oSegmentSubscriberN3.set_DataElementValue(2, 0, PatAddresln2);//"2645 MULBERRY LANE"//"Subscriber Address");

                #endregion "N3 - Subscriber Address"

                //N4: Subscriber City/State/ZIP Code
                //Purpose: To specify the geographic place of the named party
                //Example ANSI 4010: N4*Trenton*NJ*08608*US~
                //Example ANSI 5010: N4*PEACE*Ny*10023*US~
                #region "N4 - Subscriber City/State/ZIP Code"

                ediDataSegment.Set(ref oSegmentSubscriberN4, (ediDataSegment)oTransactionset.CreateDataSegment("HL(3)\\NM1\\N4"));
                if (oSegmentSubscriberN4 == null)
                {
                    oSegmentSubscriberN4 = new ediDataSegment();
                }
                //City Name 
                //Free-form text for city name
                //Use this text for the city name of the subscriber�s address.
                //oSegment.set_DataElementValue(1, 0, oSubscriber.SubscriberAddress.City);//"Colorado"
                oSegmentSubscriberN4.set_DataElementValue(1, 0, ReplaceSpecialChar(oPatient.PatientAddress.City));//"Colorado"

                //State or Province Code 
                //Code (Standard State/Province) as defined by appropriate government agency Required when the address is in the United States of America. including its terntones, or Canada. If not required by this implementation guide, do not send.
                //oSegment.set_DataElementValue(2, 0, oSubscriber.SubscriberAddress.State);//"CO"//"State");
                oSegmentSubscriberN4.set_DataElementValue(2, 0, oPatient.PatientAddress.State);


                //if (oSubscriber.SubscriberAddress.Zip != "" && oSubscriber.SubscriberAddress.Zip.Length > 5)
                //{
                //    oSubscriber.SubscriberAddress.Zip = oSubscriber.SubscriberAddress.Zip.Substring(0, 5);
                //}
                //Postal Code 
                //Code defining international postal zone code excluding punctuation and blanks (zip code for United States)
                //Required when the address is in the United States of America. including its territories, or Canada, or when a postal code exists for the country in N404. If not required by this implementation guide, do not send.
                //oSegment.set_DataElementValue(3, 0, oSubscriber.SubscriberAddress.Zip);//"803025977"
                if (oPatient.PatientAddress.Zip != "" && oPatient.PatientAddress.Zip.Length > 5)
                {
                    oPatient.PatientAddress.Zip = oPatient.PatientAddress.Zip.Substring(0, 5);
                }
                oSegmentSubscriberN4.set_DataElementValue(3, 0, oPatient.PatientAddress.Zip);//"54360"//"ZIP");

                //Country Code 
                //Code identif�ing the country
                //Use this code to specify the country of the subscribers address, if other than the United States.
                //Do not send the US County Code
                oSegmentSubscriberN4.set_DataElementValue(4, 0, "");

                #endregion "N4 - Subscriber City/State/ZIP Code"
                ClsgloRxHubGeneral.UpdateLog("Create DMG segment ");

                //DMG: Subscriber Demographic Information
                //Purpose: To supply demographic infomiation
                //Example ANSI 4010: DMG*D8*19720910*M~
                //Example ANSI 5010: DMG*D8*19720910*M~
                #region "DMG - Subscriber Demographic Information"

                ediDataSegment.Set(ref oSegmentSubscriberDMG, (ediDataSegment)oTransactionset.CreateDataSegment("HL(3)\\NM1\\DMG"));
                if (oSegmentSubscriberDMG == null)
                {
                    oSegmentSubscriberDMG = new ediDataSegment();
                }
                //Date Time Period Format Qualifier 
                //Code indicating the date format, t�me format, or date and time format.
                //Use this code to indicate the format of the date of birth that follows in DMGO2. See X12 Guide Date Expressed in Format CCYYMMDD
                //D8            Date Expressed in Format CCYYMMDD
                oSegmentSubscriberDMG.set_DataElementValue(1, 0, "D8");


                //Date Time Period 
                //Expression of a date. a time, oc range of dates, times or dates and times Use this date for :he date of birth of the individual. See X12 Guide.
                //string _SubscriberDOB = gloDateMaster.gloDate.DateAsNumber(oSubscriber.SubscriberDOB.ToShortDateString()).ToString();//GLO2011-0011225--pass the subscriber Fname, Mname, LName,DOB, Gender, ZIPcode information
                //oSegment.set_DataElementValue(2, 0, _SubscriberDOB);// "19780401"
                //send patient info instead of subscriber info, Email from D.N. dated 11 Aug 2011 sub: Surescripts Eligibility Request Change, patient DOB code again uncommented
                string _patDOB = gloDateMaster.gloDate.DateAsNumber(oPatient.DOB.ToShortDateString()).ToString(); //commented for GLO2011-0011225
                oSegmentSubscriberDMG.set_DataElementValue(2, 0, _patDOB);// oPatient.DOB.ToString()); //"19450201"//Date of Birth commented for GLO2011-0011225


                //GLO2011-0011225--pass the subscriber Fname, Mname, LName,DOB, Gender, ZIPcode information
                //Gender Code 
                //Code indicating the sex of the individual Use this code to indicate the subscribers gender See X12 Guide.
                //F             Female
                //M             Male
                if (oPatient.Gender != "")
                {
                    if (oPatient.Gender == "Female")
                    {
                        oSegmentSubscriberDMG.set_DataElementValue(3, 0, "F");//"M" //Gender
                    }
                    else if (oPatient.Gender == "Male")
                    {
                        oSegmentSubscriberDMG.set_DataElementValue(3, 0, "M");//"M" //Gender
                    }
                    //as per new "Prescription Benefit IG 2011-04-15.pdf" there is no category given for OTHERS
                    //else if (oSubscriber.SubscriberGender == "Other")
                    //{
                    //    oSegment.set_DataElementValue(3, 0, "O");//"Other"
                    //}
                }
                else
                {
                    oSegmentSubscriberDMG.set_DataElementValue(3, 0, "");//"M" //Gender
                }



                #endregion "DMG - Subscriber Demographic Information"
                ClsgloRxHubGeneral.UpdateLog("Create DTP segment ");
                //DTP: Subscriber Date
                //Purpose: To specify any or all of a date, a time, or a time period
                //*Use this segment only if subscriber is patient
                //Example ANSI 4010: DTP*472*D8*20110630~
                //Example ANSI 5010: DTP*291*D8*20091222~
                #region "DTP - Subscriber Date"

                ediDataSegment.Set(ref oSegmentSubscriberDT, (ediDataSegment)oTransactionset.CreateDataSegment("HL(3)\\NM1\\DTP"));
                if (oSegmentSubscriberDT == null)
                {
                    oSegmentSubscriberDT = new ediDataSegment();
                }
                //Date/Time Qualifier 
                //Code specifying type of date or time. or both date and time 
                //291           Plan
                //Begin and end dates of the service being rendered
                oSegmentSubscriberDT.set_DataElementValue(1, 0, "291");//"472"=Service was used


                //Date Time Period Format Qualifier 
                //Code indicating the date format, time format, or date and time format
                //D8            Date Expressed in Format CCYYMMDD
                //              *Surescrip is recommending D8. It should be within 24 hours of the date me transaction �s sent
                //RD8           Date Range expressed in Format CCYYMMDD 
                oSegmentSubscriberDT.set_DataElementValue(2, 0, "D8");

                //Date Time Period 
                //Expression of a date. a time, oc range of dates, times or dates and times
                //Use this date for the date(s) as qualified by the preceding data elements. Date expressed as CCYYMMDD
                oSegmentSubscriberDT.set_DataElementValue(3, 0, Convert.ToString(gloDateMaster.gloDate.DateAsNumber(DateTime.Now.ToShortDateString())));

                #endregion "DTP - Subscriber Date"
                ClsgloRxHubGeneral.UpdateLog("Create EQ segment ");

                //EQ: Subscriber Eligibility or Benefit Inquiry Information (Pharmacy)
                //Purpose: To specify inquired eligibility or benefit infomiation
                //Example ANSI 4010: EQ*88~ and EQ*90~
                //Example ANSI 5010: EQ*30~
                #region "EQ - Subscriber Eligibility or Benefit Inquiry Information (Pharmacy)"


                //Service Type Code 
                //Code identifying the classification of service
                //30                Health Benefit Plan Coverage Recommended by Surescripts
                //*Instead of specifying a specific service type code, this code allows the information source to respond with all the relevant service types. If other service
                //types are sent. the responder will only respond to pharmacy-related coverages.
                //ediDataSegment.Set(ref oSegment, (ediDataSegment)oTransactionset.CreateDataSegment("HL(3)\\NM1\\EQ\\EQ"));
                //oSegment.set_DataElementValue(1, 0, "88"); // Retail order Pharmacy (recommended by RxHub)


                //Service Type Code 
                //Code identifying the classification of service
                //30                Health Benefit Plan Coverage Recommended by Surescripts
                //*Instead of specifying a specific service type code, this code allows the information source to respond with all the relevant service types. If other service
                //types are sent. the responder will only respond to pharmacy-related coverages.
                //ediDataSegment.Set(ref oSegment, (ediDataSegment)oTransactionset.CreateDataSegment("HL(3)\\NM1\\EQ\\EQ"));
                //oSegment.set_DataElementValue(1, 0, "90");//Mail Order Prescription Drug

                ediDataSegment.Set(ref oSegmentEQ, (ediDataSegment)oTransactionset.CreateDataSegment("HL(3)\\NM1\\EQ\\EQ"));
                if (oSegmentEQ == null)
                {
                    oSegmentEQ = new ediDataSegment();
                }
                oSegmentEQ.set_DataElementValue(1, 0, "30"); //30                Health Benefit Plan Coverage Recommended by Surescripts 
                #endregion"EQ - Subscriber Eligibility or Benefit Inquiry Information (Pharmacy)"




                #region  " Save EDI File "
                ClsgloRxHubGeneral.UpdateLog("Before File Save");
                //Save to a file
                sPath = gloRxHub.ClsgloRxHubGeneral.gnstrApplicationFilePath + "Outbox\\";
                sEdiFile = "EDI270_" + DateTime.Now.Date.ToString("MMddyyyy") + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + DateTime.Now.Millisecond.ToString() + ".X12"; //Convert.ToString(gloDateMaster.gloDate.DateAsNumber(DateTime.Now.ToShortDateString())) + Convert.ToString(gloDateMaster.gloTime.TimeAsNumber(DateTime.Now.ToLocalTime().ToShortTimeString())).Trim();
                string EdiFile = "";
                EdiFile = sPath + sEdiFile;
                oEdiDoc.Save(EdiFile);//sPath + sEdiFile
                ClsgloRxHubGeneral.UpdateLog("After File Save");

                //strFileName = EdiFile;
                //_270FilePath = strFileName;
                //txtEDIOutput.Text = oEdiDoc.GetEdiString();
                #endregion  " Save EDI File "
                ClsgloRxHubGeneral.UpdateLog("Before Database Entry");
                #region  " Save 270 Info to database "

                //oclsgloRxHubDBLayer.Connect(gloRxHub.ClsgloRxHubGeneral.ConnectionString);

                oclsgloRxHubDBLayer.InsertEDIResquest270_5010("gsp_InUpRxH_270Request_Details", strControlNo, BHT_TransactionRef, _PatientId);

                //oclsgloRxHubDBLayer.Disconnect();

                #endregion  " Save 270 Info to database "
                ClsgloRxHubGeneral.UpdateLog("After Database Entry");
                ClsgloRxHubGeneral.UpdateLog("End Try Block ");
                return EdiFile;
                //return _270FilePath;
            }
            catch (System.Runtime.CompilerServices.RuntimeWrappedException Rex)
            {
                string _strEx = "";
                ediException oException = null;
                oException = (ediException)Rex.WrappedException;
                _strEx = oException.get_Description();
                //gloAuditTrail.gloAuditTrail.ExceptionLog(_strEx, true);
                gloAuditTrail.gloAuditTrail.ExceptionLog(gloAuditTrail.ActivityModule.Prescription, gloAuditTrail.ActivityCategory.CreatePrescription, gloAuditTrail.ActivityType.General, _strEx, gloAuditTrail.ActivityOutCome.Failure);

                if (oclsgloRxHubDBLayer != null)
                {
                    oclsgloRxHubDBLayer.Disconnect();
                    oclsgloRxHubDBLayer.Dispose();
                    oclsgloRxHubDBLayer = null;
                }
                if (oclsgloRxHubDBLayer != null)
                {
                    oclsgloRxHubDBLayer.Disconnect();
                    oclsgloRxHubDBLayer.Dispose();
                    oclsgloRxHubDBLayer = null;
                }
                if (oSegmentEQ != null)
                {
                    oSegmentEQ.Dispose();
                    oSegmentEQ = null;
                }
                if (oSegmentSubscriberDT != null)
                {
                    oSegmentSubscriberDT.Dispose();
                    oSegmentSubscriberDT = null;
                }
                if (oSegmentSubscriberDMG != null)
                {
                    oSegmentSubscriberDMG.Dispose();
                    oSegmentSubscriberDMG = null;
                }
                if (oSegmentSubscriberN4 != null)
                {
                    oSegmentSubscriberN4.Dispose();
                    oSegmentSubscriberN4 = null;
                }
                if (oSegmentSubscriberN3 != null)
                {
                    oSegmentSubscriberN3.Dispose();
                    oSegmentSubscriberN3 = null;
                }
                if (oSegmentSubscriberREF != null)
                {
                    oSegmentSubscriberREF.Dispose();
                    oSegmentSubscriberREF = null;
                }
                if (oSegmentSubscriberNM1 != null)
                {
                    oSegmentSubscriberNM1.Dispose();
                    oSegmentSubscriberNM1 = null;
                }
                if (oSegmentSubscriberHL != null)
                {
                    oSegmentSubscriberHL.Dispose();
                    oSegmentSubscriberHL = null;
                }
                if (oSegmentN4 != null)
                {
                    oSegmentN4.Dispose();
                    oSegmentN4 = null;
                }
                if (oSegmentN3 != null)
                {
                    oSegmentN3.Dispose();
                    oSegmentN3 = null;
                }
                if (oSegmentREF != null)
                {
                    oSegmentREF.Dispose();
                    oSegmentREF = null;
                }
                if (oSegmentNM2 != null)
                {
                    oSegmentNM2.Dispose();
                    oSegmentNM2 = null;
                }
                if (oSegmentHL2 != null)
                {
                    oSegmentHL2.Dispose();
                    oSegmentHL2 = null;
                }
                if (oSegmentNM1 != null)
                {
                    oSegmentNM1.Dispose();
                    oSegmentNM1 = null;
                }
                if (oSegmentHL != null)
                {
                    oSegmentHL.Dispose();
                    oSegmentHL = null;
                }
                if (oSegmentBHT != null)
                {
                    oSegmentBHT.Dispose();
                    oSegmentBHT = null;
                }
                if (oSegmentST != null)
                {
                    oSegmentST.Dispose();
                    oSegmentST = null;
                }
                if (oSegmentGS != null)
                {
                    oSegmentGS.Dispose();
                    oSegmentGS = null;
                }
                if (oSegmentISA != null)
                {
                    oSegmentISA.Dispose();
                    oSegmentISA = null;
                }
                if (oTransactionset != null)
                {
                    oTransactionset.Dispose();
                    oTransactionset = null;
                }
                if (oGroup != null)
                {
                    oGroup.Dispose();
                    oGroup = null;
                }
                if (oInterchange != null)
                {
                    oInterchange.Dispose();
                    oInterchange = null;
                }

                FreeEDIObject(ref oEdiDoc);
                throw Rex;
            }
            catch (Exception ex)
            {
                if (oclsgloRxHubDBLayer != null)
                {
                    oclsgloRxHubDBLayer.Disconnect();
                    oclsgloRxHubDBLayer.Dispose();
                    oclsgloRxHubDBLayer = null;
                }
                //SLR: Don't dispose, since it is a reference from Patient.subscriber[i] 
                //if (oSubscriber != null)
                //{
                //    oSubscriber.Dispose();
                //    oSubscriber = null;
                //}
                if (oclsgloRxHubDBLayer != null)
                {
                    oclsgloRxHubDBLayer.Disconnect();
                    oclsgloRxHubDBLayer.Dispose();
                    oclsgloRxHubDBLayer = null;
                }
                if (oSegmentEQ != null)
                {
                    oSegmentEQ.Dispose();
                    oSegmentEQ = null;
                }
                if (oSegmentSubscriberDT != null)
                {
                    oSegmentSubscriberDT.Dispose();
                    oSegmentSubscriberDT = null;
                }
                if (oSegmentSubscriberDMG != null)
                {
                    oSegmentSubscriberDMG.Dispose();
                    oSegmentSubscriberDMG = null;
                }
                if (oSegmentSubscriberN4 != null)
                {
                    oSegmentSubscriberN4.Dispose();
                    oSegmentSubscriberN4 = null;
                }
                if (oSegmentSubscriberN3 != null)
                {
                    oSegmentSubscriberN3.Dispose();
                    oSegmentSubscriberN3 = null;
                }
                if (oSegmentSubscriberREF != null)
                {
                    oSegmentSubscriberREF.Dispose();
                    oSegmentSubscriberREF = null;
                }
                if (oSegmentSubscriberNM1 != null)
                {
                    oSegmentSubscriberNM1.Dispose();
                    oSegmentSubscriberNM1 = null;
                }
                if (oSegmentSubscriberHL != null)
                {
                    oSegmentSubscriberHL.Dispose();
                    oSegmentSubscriberHL = null;
                }
                if (oSegmentN4 != null)
                {
                    oSegmentN4.Dispose();
                    oSegmentN4 = null;
                }
                if (oSegmentN3 != null)
                {
                    oSegmentN3.Dispose();
                    oSegmentN3 = null;
                }
                if (oSegmentREF != null)
                {
                    oSegmentREF.Dispose();
                    oSegmentREF = null;
                }
                if (oSegmentNM2 != null)
                {
                    oSegmentNM2.Dispose();
                    oSegmentNM2 = null;
                }
                if (oSegmentHL2 != null)
                {
                    oSegmentHL2.Dispose();
                    oSegmentHL2 = null;
                }
                if (oSegmentNM1 != null)
                {
                    oSegmentNM1.Dispose();
                    oSegmentNM1 = null;
                }
                if (oSegmentHL != null)
                {
                    oSegmentHL.Dispose();
                    oSegmentHL = null;
                }
                if (oSegmentBHT != null)
                {
                    oSegmentBHT.Dispose();
                    oSegmentBHT = null;
                }
                if (oSegmentST != null)
                {
                    oSegmentST.Dispose();
                    oSegmentST = null;
                }
                if (oSegmentGS != null)
                {
                    oSegmentGS.Dispose();
                    oSegmentGS = null;
                }
                if (oSegmentISA != null)
                {
                    oSegmentISA.Dispose();
                    oSegmentISA = null;
                }
                if (oTransactionset != null)
                {
                    oTransactionset.Dispose();
                    oTransactionset = null;
                }
                if (oGroup != null)
                {
                    oGroup.Dispose();
                    oGroup = null;
                }
                if (oInterchange != null)
                {
                    oInterchange.Dispose();
                    oInterchange = null;
                }
                FreeEDIObject(ref oEdiDoc);
                gloAuditTrail.gloAuditTrail.ExceptionLog(gloAuditTrail.ActivityModule.Prescription, gloAuditTrail.ActivityCategory.CreatePrescription, gloAuditTrail.ActivityType.General, ex.ToString(), gloAuditTrail.ActivityOutCome.Failure);
                throw ex;
            }
            finally
            {
                ClsgloRxHubGeneral.UpdateLog("End Generate270_5010 ");
                if (oclsgloRxHubDBLayer != null)
                {
                    oclsgloRxHubDBLayer.Disconnect();
                    oclsgloRxHubDBLayer.Dispose();
                    oclsgloRxHubDBLayer = null;
                }
                if (oSegmentEQ!=null)
                {
                    oSegmentEQ.Dispose();
                    oSegmentEQ = null;
                }
                if (oSegmentSubscriberDT != null)
                {
                    oSegmentSubscriberDT.Dispose();
                    oSegmentSubscriberDT = null;
                }
                if (oSegmentSubscriberDMG != null)
                {
                    oSegmentSubscriberDMG.Dispose();
                    oSegmentSubscriberDMG = null;
                }
                if (oSegmentSubscriberN4 != null)
                {
                    oSegmentSubscriberN4.Dispose();
                    oSegmentSubscriberN4 = null;
                }
                if (oSegmentSubscriberN3 != null)
                {
                    oSegmentSubscriberN3.Dispose();
                    oSegmentSubscriberN3 = null;
                }
                if (oSegmentSubscriberREF != null)
                {
                    oSegmentSubscriberREF.Dispose();
                    oSegmentSubscriberREF = null;
                }
                if (oSegmentSubscriberNM1 != null)
                {
                    oSegmentSubscriberNM1.Dispose();
                    oSegmentSubscriberNM1 = null;
                }
                if (oSegmentSubscriberHL != null)
                {
                    oSegmentSubscriberHL.Dispose();
                    oSegmentSubscriberHL = null;
                }
                if (oSegmentN4 != null)
                {
                    oSegmentN4.Dispose();
                    oSegmentN4 = null;
                }
                if (oSegmentN3 != null)
                {
                    oSegmentN3.Dispose();
                    oSegmentN3 = null;
                }
                if (oSegmentREF != null)
                {
                    oSegmentREF.Dispose();
                    oSegmentREF = null;
                }
                if (oSegmentNM2 != null)
                {
                    oSegmentNM2.Dispose();
                    oSegmentNM2 = null;
                }
                if (oSegmentHL2 != null)
                {
                    oSegmentHL2.Dispose();
                    oSegmentHL2 = null;
                }
                if (oSegmentNM1 != null)
                {
                    oSegmentNM1.Dispose();
                    oSegmentNM1 = null;
                }
                if (oSegmentHL != null)
                {
                    oSegmentHL.Dispose();
                    oSegmentHL = null;
                }
                if (oSegmentBHT != null)
                {
                    oSegmentBHT.Dispose();
                    oSegmentBHT = null;
                }
                if (oSegmentST != null)
                {
                    oSegmentST.Dispose();
                    oSegmentST = null;
                }
                if (oSegmentGS != null)
                {
                    oSegmentGS.Dispose();
                    oSegmentGS = null;
                }
                if (oSegmentISA != null)
                {
                    oSegmentISA.Dispose();
                    oSegmentISA = null;
                }
                if (oTransactionset != null)
                {
                    oTransactionset.Dispose();
                    oTransactionset = null;
                }
                if (oGroup != null)
                {
                    oGroup.Dispose();
                    oGroup = null;
                }
                if (oInterchange != null)
                {
                    oInterchange.Dispose();
                    oInterchange = null;
                }

                FreeEDIObject(ref oEdiDoc);

            }


        }

        private string ReplaceSpecialChar(string sTempString)
        {
            return Regex.Replace(sTempString, "[~*:^]", "");
        }
        public bool IsValidPatientInfo_270(Int64 PatientID, ref long nLoginProviderid, bool bShowMessage = true)
        {
            clsgloRxHubDBLayer ogloRxHubDBLayer = new clsgloRxHubDBLayer();
            DataSet oPatInfo270ds = null;
            StringBuilder strInfo = new StringBuilder();
            string Gender=string.Empty;
            string City = string.Empty;
            string State = string.Empty;
            string Zip = string.Empty;
            try
            {
                ogloRxHubDBLayer.Connect(ClsgloRxHubGeneral.ConnectionString);
                oPatInfo270ds = ogloRxHubDBLayer.GetPatientInformation_270(PatientID,ref nLoginProviderid);

                if (oPatInfo270ds.Tables.Count > 0)
                {
                   

                    #region "Patient Table"

                    if (oPatInfo270ds.Tables[0].Rows.Count > 0) //table[0] has Patient table information in it
                    {
                        //Fill Patient object

                        if (oPatInfo270ds.Tables[0].Rows[0]["FName"].ToString().Length > 35)
                        {
                            strInfo.Append("Patient First Name(35),");                            
                        }
                       
                        //oPatient.FirstName = oPatInfo270ds.Tables[0].Rows[0]["FName"].ToString();
                        if (oPatInfo270ds.Tables[0].Rows[0]["LName"].ToString().Length > 35)
                        {
                            strInfo.Append("Patient Last Name(35),");
                        }
                       
                       
                        if (oPatInfo270ds.Tables[0].Rows[0]["Address1"].ToString().Length > 35)
                        {
                            strInfo.Append("Patient Address Line 1(35),");
                        }
                     

                        if (oPatInfo270ds.Tables[0].Rows[0]["Address2"].ToString().Length > 35)
                        {
                            strInfo.Append("Patient Address Line 2(35),");
                        }
                        
                        //oPatient.PatientAddress.AddressLine2 = oPatInfo270ds.Tables[0].Rows[0]["Address2"].ToString();
                        if (oPatInfo270ds.Tables[0].Rows[0]["City"].ToString().Length > 35)
                        {
                            strInfo.Append("Patient City(35),");
                        }

                        Gender = Convert.ToString(oPatInfo270ds.Tables[0].Rows[0]["Gender"]);
                        City = Convert.ToString(oPatInfo270ds.Tables[0].Rows[0]["City"]);
                        State = Convert.ToString(oPatInfo270ds.Tables[0].Rows[0]["State"]);
                        Zip = Convert.ToString(oPatInfo270ds.Tables[0].Rows[0]["Zip"]);
                    }

                    #endregion "Patient Table"


                    #region "Provider_Mst Table"

                    if (oPatInfo270ds.Tables[2].Rows.Count > 0) //table[2] has Provider_Mst table information in it
                    {
                        
                        if (oPatInfo270ds.Tables[2].Rows[0]["FName"].ToString().Length > 35)
                        {
                            strInfo.Append("Provider First Name(35),");
                        }
                        
                        if (oPatInfo270ds.Tables[2].Rows[0]["LName"].ToString().Length > 35)
                        {
                            strInfo.Append("Provider Last Name(35),");                            
                        }
                        

                        if (oPatInfo270ds.Tables[2].Rows[0]["Street"].ToString().Length > 35)
                        {
                            strInfo.Append("Provider Address Line 1(35),");
                        }
                       
                        //oPatient.Provider.ProviderAddress.AddressLine1 = oPatInfo270ds.Tables[2].Rows[0]["Street"].ToString();
                        if (oPatInfo270ds.Tables[2].Rows[0]["Address"].ToString().Length > 35)
                        {
                            strInfo.Append("Provider Address Line 2(35),");
                        }
                       
                        //oPatient.Provider.ProviderAddress.AddressLine2 = oPatInfo270ds.Tables[2].Rows[0]["Address"].ToString();
                        if (oPatInfo270ds.Tables[2].Rows[0]["City"].ToString().Length > 35)
                        {
                            strInfo.Append("Provider City(35),");
                        }
                        
                    }

                    #endregion "Provider_Mst Table"

                    #region "Validate Patient info"

                   // string sMessage = validate270File_PatientInfo(oPatInfo270ds.Tables[0].Rows[0]["FName"].ToString(), oPatInfo270ds.Tables[0].Rows[0]["LName"].ToString(), oPatInfo270ds.Tables[0].Rows[0]["MName"].ToString(), oPatInfo270ds.Tables[0].Rows[0]["Gender"].ToString(), oPatInfo270ds.Tables[0].Rows[0]["Address1"].ToString(), oPatInfo270ds.Tables[0].Rows[0]["City"].ToString(), oPatInfo270ds.Tables[0].Rows[0]["State"].ToString(), oPatInfo270ds.Tables[0].Rows[0]["Zip"].ToString(), oPatInfo270ds.Tables[2].Rows[0]["FName"].ToString(), oPatInfo270ds.Tables[2].Rows[0]["LName"].ToString(), oPatInfo270ds.Tables[2].Rows[0]["MName"].ToString());
                      string sMessage = validate270File_PatientInfo(Gender, City, State,  Zip);
                    if (sMessage != string.Empty)
                    {
                        if (bShowMessage)
                        {
                            MessageBox.Show(sMessage, "gloEMR", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }
                        sMessage = string.Empty;
                        return false;
                    }
                    #endregion ""

                    string strMessage = "";
                    if (strInfo.Length > 0)
                    {
                        if (!string.IsNullOrEmpty(strInfo.ToString().Trim()))
                        {
                            if (strInfo.ToString().EndsWith(","))
                            {

                                strMessage = strInfo.ToString().Trim().Substring(0, ((strInfo.ToString().Trim().Length) - 1));
                                strInfo = new System.Text.StringBuilder();
                                strInfo.Append(strMessage);
                            }
                        }
                    }
                    if (strInfo.ToString().Length > 0)
                    {
                        if (bShowMessage)
                        {
                            if (MessageBox.Show("Following data fields exceed number of characters allowed in x12 standards and will therefore be truncated before sending to Surescripts and PBM�s (Allowed characters are shown in parenthesis) " + System.Environment.NewLine + System.Environment.NewLine + System.Environment.NewLine + strMessage + System.Environment.NewLine + System.Environment.NewLine + System.Environment.NewLine + "Do you want to continue?", "gloEMR", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == System.Windows.Forms.DialogResult.No)
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return true;
                        }
                    }

                    if (oPatInfo270ds.Tables[2].Rows[0]["NPI"].ToString() == "" && oPatInfo270ds.Tables[2].Rows[0]["DEANumber"].ToString() == "")
                    {
                        MessageBox.Show("Provider must have NPI or DEA Number.", "gloEMR", MessageBoxButtons.OK);
                        return false;
                    }

                }

          
                ogloRxHubDBLayer.Disconnect();
                return true;
            }
            catch (Exception ex)
            {

                gloAuditTrail.gloAuditTrail.ExceptionLog(gloAuditTrail.ActivityModule.Prescription, gloAuditTrail.ActivityCategory.CreatePrescription, gloAuditTrail.ActivityType.General, ex.ToString(), gloAuditTrail.ActivityOutCome.Failure);
                return false;
                throw ex;
            }
            finally
            {
                //SLR: Free oPatInfo270ds
                if (oPatInfo270ds != null)
                {
                    oPatInfo270ds.Dispose();
                    oPatInfo270ds = null;
                }
                if (ogloRxHubDBLayer != null)
                {
                    ogloRxHubDBLayer.Dispose();
                    ogloRxHubDBLayer = null;
                }
            }
        }

        public bool FillPatientInfo_270(Int64 PatientID, ref long nLoginProviderid, bool bShowMessage = false)
        {
            clsgloRxHubDBLayer ogloRxHubDBLayer = new clsgloRxHubDBLayer();
            DataSet oPatInfo270ds = null;
            StringBuilder strInfo = new StringBuilder();

            try
            {
                ogloRxHubDBLayer.Connect(ClsgloRxHubGeneral.ConnectionString);
                oPatInfo270ds = ogloRxHubDBLayer.GetPatientInformation_270(PatientID, ref nLoginProviderid);
                ogloRxHubDBLayer.Disconnect();

                if (oPatInfo270ds.Tables.Count > 0)
                {

                    if (oPatient != null)
                    {
                        oPatient.Dispose();
                    }
                    oPatient = new ClsPatient();

                    #region "Patient Table"

                    if (oPatInfo270ds.Tables[0].Rows.Count > 0) //table[0] has Patient table information in it
                    {


                        oPatient.PatientID = Convert.ToInt64(oPatInfo270ds.Tables[0].Rows[0]["PatientID"]);
                        if (oPatInfo270ds.Tables[0].Rows[0]["FName"].ToString().Length > 35)
                        {
                            strInfo.Append("Patient First Name(35),");
                            oPatient.FirstName = oPatInfo270ds.Tables[0].Rows[0]["FName"].ToString().Substring(0, 35);
                        }
                        else
                        {
                            oPatient.FirstName = oPatInfo270ds.Tables[0].Rows[0]["FName"].ToString();
                        }


                        if (oPatInfo270ds.Tables[0].Rows[0]["LName"].ToString().Length > 35)
                        {
                            strInfo.Append("Patient Last Name(35),");
                            oPatient.LastName = oPatInfo270ds.Tables[0].Rows[0]["LName"].ToString().Substring(0, 35);
                        }
                        else
                        {
                            oPatient.LastName = oPatInfo270ds.Tables[0].Rows[0]["LName"].ToString();
                        }


                        oPatient.MiddleName = oPatInfo270ds.Tables[0].Rows[0]["MName"].ToString();
                        oPatient.DOB = Convert.ToDateTime(oPatInfo270ds.Tables[0].Rows[0]["DOB"]);

                        oPatient.Provider.ProviderID = Convert.ToInt64(oPatInfo270ds.Tables[0].Rows[0]["ProviderID"]);
                        oPatient.SSN = "000000000"; //oPatInfo270ds.Tables[0].Rows[0]["SSN"].ToString(); as per yaw sir discussion SSN no should be sent as "000000000" 

                        if (oPatInfo270ds.Tables[0].Rows[0]["Address1"].ToString().Length > 35)
                        {
                            strInfo.Append("Patient Address Line 1(35),");
                            oPatient.PatientAddress.AddressLine1 = oPatInfo270ds.Tables[0].Rows[0]["Address1"].ToString().Substring(0, 35);
                        }
                        else
                        {
                            oPatient.PatientAddress.AddressLine1 = oPatInfo270ds.Tables[0].Rows[0]["Address1"].ToString();
                        }


                        if (oPatInfo270ds.Tables[0].Rows[0]["Address2"].ToString().Length > 35)
                        {
                            strInfo.Append("Patient Address Line 2(35),");
                            oPatient.PatientAddress.AddressLine2 = oPatInfo270ds.Tables[0].Rows[0]["Address2"].ToString().Substring(0, 35);
                        }
                        else
                        {
                            oPatient.PatientAddress.AddressLine2 = oPatInfo270ds.Tables[0].Rows[0]["Address2"].ToString();
                        }

                        oPatient.Gender = oPatInfo270ds.Tables[0].Rows[0]["Gender"].ToString();
                        if (oPatInfo270ds.Tables[0].Rows[0]["City"].ToString().Length > 35)
                        {
                            strInfo.Append("Patient City(35),");
                            oPatient.PatientAddress.City = oPatInfo270ds.Tables[0].Rows[0]["City"].ToString().Substring(0, 35);
                        }
                        else
                        {
                            oPatient.PatientAddress.City = oPatInfo270ds.Tables[0].Rows[0]["City"].ToString();
                        }


                        oPatient.PatientAddress.State = oPatInfo270ds.Tables[0].Rows[0]["State"].ToString();
                        oPatient.PatientAddress.Zip = oPatInfo270ds.Tables[0].Rows[0]["Zip"].ToString();

                        string sMessage = validate270File_PatientInfo(oPatient.Gender, oPatient.PatientAddress.City, oPatient.PatientAddress.State, oPatient.PatientAddress.Zip);
                        if (sMessage != string.Empty)
                        {
                            if (bShowMessage)
                            {
                                MessageBox.Show(sMessage, "gloEMR", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            }
                            sMessage = string.Empty;
                            return false;
                        }

                        oPatient.PatientContact.Phone = oPatInfo270ds.Tables[0].Rows[0]["Phone"].ToString();
                        oPatient.PatientContact.Email = oPatInfo270ds.Tables[0].Rows[0]["Email"].ToString();
                        oPatient.PatientContact.Fax = oPatInfo270ds.Tables[0].Rows[0]["Fax"].ToString();
                        oPatient.PatientContact.Mobile = oPatInfo270ds.Tables[0].Rows[0]["Mobile"].ToString();

                    }

                    #endregion "Patient Table"

                    #region "Provider_Mst Table"

                    if (oPatInfo270ds.Tables[2].Rows.Count > 0) //table[2] has Provider_Mst table information in it
                    {
                        //Fill Provider object
                        oPatient.Provider.ProviderDEA = oPatInfo270ds.Tables[2].Rows[0]["DEANumber"].ToString();
                        oPatient.Provider.ProviderNPI = oPatInfo270ds.Tables[2].Rows[0]["NPI"].ToString();

                        if (oPatient.Provider.ProviderNPI == "" && oPatient.Provider.ProviderDEA == "")
                        {
                            if (bShowMessage)
                            {
                                MessageBox.Show("Provider must have NPI or DEA Number to process RxEligibility.", "gloEMR", MessageBoxButtons.OK);
                            }
                            return false;
                        }

                        if (oPatInfo270ds.Tables[2].Rows[0]["FName"].ToString().Length > 35)
                        {
                            strInfo.Append("Provider First Name(35),");
                            oPatient.Provider.ProviderFirstName = oPatInfo270ds.Tables[2].Rows[0]["FName"].ToString();
                        }
                        else
                        {
                            oPatient.Provider.ProviderFirstName = oPatInfo270ds.Tables[2].Rows[0]["FName"].ToString();
                        }
                        if (oPatInfo270ds.Tables[2].Rows[0]["LName"].ToString().Length > 35)
                        {
                            strInfo.Append("Provider Last Name(35),");
                            oPatient.Provider.ProviderLastName = oPatInfo270ds.Tables[2].Rows[0]["LName"].ToString().Substring(0, 35);
                        }
                        else
                        {
                            oPatient.Provider.ProviderLastName = oPatInfo270ds.Tables[2].Rows[0]["LName"].ToString();
                        }
                        oPatient.Provider.ProviderMiddleName = oPatInfo270ds.Tables[2].Rows[0]["MName"].ToString();
                        oPatient.Provider.Gender = oPatInfo270ds.Tables[2].Rows[0]["Gender"].ToString();

                        if (oPatInfo270ds.Tables[2].Rows[0]["Street"].ToString().Length > 35)
                        {
                            strInfo.Append("Provider Address Line 1(35),");
                            oPatient.Provider.ProviderAddress.AddressLine1 = oPatInfo270ds.Tables[2].Rows[0]["Street"].ToString().Substring(0, 35);
                        }
                        else
                        {
                            oPatient.Provider.ProviderAddress.AddressLine1 = oPatInfo270ds.Tables[2].Rows[0]["Street"].ToString();
                        }
                        if (oPatInfo270ds.Tables[2].Rows[0]["Address"].ToString().Length > 35)
                        {
                            strInfo.Append("Provider Address Line 2(35),");
                            oPatient.Provider.ProviderAddress.AddressLine2 = oPatInfo270ds.Tables[2].Rows[0]["Address"].ToString().Substring(0, 35);
                        }
                        else
                        {
                            oPatient.Provider.ProviderAddress.AddressLine2 = oPatInfo270ds.Tables[2].Rows[0]["Address"].ToString();
                        }
                        if (oPatInfo270ds.Tables[2].Rows[0]["City"].ToString().Length > 35)
                        {
                            strInfo.Append("Provider City(35),");
                            oPatient.Provider.ProviderAddress.City = oPatInfo270ds.Tables[2].Rows[0]["City"].ToString().Substring(0, 35);
                        }
                        else
                        {
                            oPatient.Provider.ProviderAddress.City = oPatInfo270ds.Tables[2].Rows[0]["City"].ToString();
                        }
                        oPatient.Provider.ProviderAddress.State = oPatInfo270ds.Tables[2].Rows[0]["State"].ToString();
                        oPatient.Provider.ProviderAddress.Zip = oPatInfo270ds.Tables[2].Rows[0]["Zip"].ToString();
                        oPatient.Provider.ProviderContactDtl.Phone = oPatInfo270ds.Tables[2].Rows[0]["PhoneNo"].ToString();
                        oPatient.Provider.ProviderContactDtl.Fax = oPatInfo270ds.Tables[2].Rows[0]["Fax"].ToString().Replace("-", "");
                        oPatient.Provider.ProviderContactDtl.Mobile = oPatInfo270ds.Tables[2].Rows[0]["MobileNo"].ToString();
                        oPatient.Provider.ProviderContactDtl.Email = oPatInfo270ds.Tables[2].Rows[0]["Email"].ToString();
                        oPatient.Provider.ClinicName = oPatInfo270ds.Tables[2].Rows[0]["ClinicName"].ToString();
                        oPatient.Provider.AMASpecialtyCode = oPatInfo270ds.Tables[2].Rows[0]["SpeacailtyType"].ToString();
                        oPatient.Provider.ProviderPrefix = oPatInfo270ds.Tables[2].Rows[0]["Prefix"].ToString();
                    }

                    #endregion "Provider_Mst Table"

                    #region "Validate 270 Information "

                    string strMessage = "";
                    if (strInfo.Length > 0)
                    {
                        if (!string.IsNullOrEmpty(strInfo.ToString().Trim()))
                        {
                            if (strInfo.ToString().EndsWith(","))
                            {
                                strMessage = strInfo.ToString().Trim().Substring(0, ((strInfo.ToString().Trim().Length) - 1));
                                strInfo = new System.Text.StringBuilder();
                                strInfo.Append(strMessage);
                            }
                        }
                    }
                    if (strInfo.ToString().Length > 0)
                    {
                        if (bShowMessage)
                        {
                            if (MessageBox.Show("Following data fields exceed number of characters allowed in x12 standards and will therefore be truncated before sending to Surescripts and PBM�s (Allowed characters are shown in parenthesis) " + System.Environment.NewLine + System.Environment.NewLine + System.Environment.NewLine + strMessage + System.Environment.NewLine + System.Environment.NewLine + System.Environment.NewLine + "Do you want to continue?", "gloEMR", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == System.Windows.Forms.DialogResult.No)
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return true;
                        }
                    }

                    #endregion

                    #region "Patient_Dtl Table"

                    if (oPatInfo270ds.Tables[1].Rows.Count > 0) //table[1] has Patient_Dtl table information in it
                    {
                        //Fill Pharmacy object
                        oPatient.Pharmacy.StoreName = oPatInfo270ds.Tables[1].Rows[0]["StoreName"].ToString();
                        oPatient.Pharmacy.PhramacyAddress.AddressLine1 = oPatInfo270ds.Tables[1].Rows[0]["AddressLine1"].ToString();
                        oPatient.Pharmacy.PhramacyAddress.AddressLine2 = oPatInfo270ds.Tables[1].Rows[0]["AddressLine2"].ToString();
                        oPatient.Pharmacy.PhramacyAddress.City = oPatInfo270ds.Tables[1].Rows[0]["City"].ToString();
                        oPatient.Pharmacy.PhramacyAddress.State = oPatInfo270ds.Tables[1].Rows[0]["State"].ToString();
                        oPatient.Pharmacy.PhramacyAddress.Zip = oPatInfo270ds.Tables[1].Rows[0]["Zip"].ToString();
                        oPatient.Pharmacy.PharmacyContactDetails.Phone = oPatInfo270ds.Tables[1].Rows[0]["Phone"].ToString();
                        oPatient.Pharmacy.PharmacyContactDetails.Email = oPatInfo270ds.Tables[1].Rows[0]["Email"].ToString();
                    }

                    #endregion "Patient_Dtl Table"




                    #region "RxH_271Master Table"

                    if (oPatInfo270ds.Tables[3].Rows.Count > 0) //table[3] has RxH_271Master table information in it
                    {

                        oPatient.RxH271Master.PayerName = oPatInfo270ds.Tables[3].Rows[0]["PayerName"].ToString();
                        oPatient.RxH271Master.PayerParticipantId = oPatInfo270ds.Tables[3].Rows[0]["PayerID"].ToString();
                        oPatient.RxH271Master.MemberID = oPatInfo270ds.Tables[3].Rows[0]["PBMMemberID"].ToString();
                        oPatient.RxH271Master.CardHolderId = oPatInfo270ds.Tables[3].Rows[0]["CardHolderID"].ToString();
                        oPatient.RxH271Master.CardHolderName = oPatInfo270ds.Tables[3].Rows[0]["CardHolderName"].ToString();
                        oPatient.RxH271Master.GroupId = oPatInfo270ds.Tables[3].Rows[0]["GroupID"].ToString();
                        oPatient.RxH271Master.RelationshipCode = oPatInfo270ds.Tables[3].Rows[0]["RelationshipCode"].ToString();

                    }

                    #endregion "RxH_271Master Table"

                    #region "Patientinsurance_DTL Table"
                    //-------Bug #69460: 00000709: RxElig issue. Subscriber info not required for Rx-eligibility, so it is commented
                    //if (oPatInfo270ds.Tables[4].Rows.Count > 0) //table[4] has Patientinsurance_DTL table information in it
                    //{

                    //    for (int nRwCnt = 0; nRwCnt < oPatInfo270ds.Tables[4].Rows.Count; nRwCnt++)
                    //    {
                    //        ClsSubscriber oclsSubscriber = new ClsSubscriber();
                    //        oclsSubscriber.SubscriberID = oPatInfo270ds.Tables[4].Rows[nRwCnt]["SubscriberID"].ToString();
                    //        oclsSubscriber.SubscriberFirstName = oPatInfo270ds.Tables[4].Rows[nRwCnt]["SubFName"].ToString();
                    //        oclsSubscriber.SubscriberLastName = oPatInfo270ds.Tables[4].Rows[nRwCnt]["SubLName"].ToString();
                    //        oclsSubscriber.SubscriberMiddleName = oPatInfo270ds.Tables[4].Rows[nRwCnt]["SubMName"].ToString();

                    //        //GLO2011-0011225
                    //        oclsSubscriber.SubscriberDOB = Convert.ToDateTime(oPatInfo270ds.Tables[4].Rows[nRwCnt]["SubDOB"]);
                    //        oclsSubscriber.SubscriberGender = oPatInfo270ds.Tables[4].Rows[nRwCnt]["SubGender"].ToString();

                    //        oclsSubscriber.SubscriberAddress.AddressLine1 = oPatInfo270ds.Tables[4].Rows[nRwCnt]["SubscriberAddr1"].ToString();
                    //        oclsSubscriber.SubscriberAddress.AddressLine2 = oPatInfo270ds.Tables[4].Rows[nRwCnt]["SubscriberAddr2"].ToString();
                    //        oclsSubscriber.SubscriberAddress.State = oPatInfo270ds.Tables[4].Rows[nRwCnt]["SubscriberState"].ToString();
                    //        oclsSubscriber.SubscriberAddress.City = oPatInfo270ds.Tables[4].Rows[nRwCnt]["SubscriberCity"].ToString();
                    //        oclsSubscriber.SubscriberAddress.Zip = oPatInfo270ds.Tables[4].Rows[nRwCnt]["SubscriberZip"].ToString();
                    //        oclsSubscriber.InsuranceID = Convert.ToInt64(oPatInfo270ds.Tables[4].Rows[nRwCnt]["InsuranceID"]);
                    //        oclsSubscriber.SubscriberContactDtl.Phone = oPatInfo270ds.Tables[4].Rows[nRwCnt]["Phone"].ToString();
                    //        oclsSubscriber.SubscriberContactDtl.Email = oPatInfo270ds.Tables[4].Rows[nRwCnt]["Email"].ToString(); ;
                    //        //oclsSubscriber.SubscriberContactDtl .Phone ;
                    //        //oclsSubscriber .SubscriberContactDtl .Mobile ;
                    //        //oclsSubscriber .SubscriberContactDtl .Fax ;
                    //        oclsSubscriber.InsuranceName = oPatInfo270ds.Tables[4].Rows[0]["InsuranceName"].ToString();

                    //        oclsSubscriber.RelationShip = oPatInfo270ds.Tables[4].Rows[0]["RelationShip"].ToString();
                    //        oPatient.Subscriber.Add(oclsSubscriber);
                    //        oclsSubscriber = null;
                    //    }


                    //}

                    #endregion "Patientinsurance_DTL Table"

                }


                return true;
            }
            catch (Exception ex)
            {

                gloAuditTrail.gloAuditTrail.ExceptionLog(gloAuditTrail.ActivityModule.Prescription, gloAuditTrail.ActivityCategory.CreatePrescription, gloAuditTrail.ActivityType.General, ex.ToString(), gloAuditTrail.ActivityOutCome.Failure);
                return false;
                throw ex;
            }
            finally
            {
                //SLR: Free oPatInfo270ds
                if (oPatInfo270ds != null)
                {
                    oPatInfo270ds.Dispose();
                    oPatInfo270ds = null;
                }
                if (ogloRxHubDBLayer != null)
                {
                    ogloRxHubDBLayer.Dispose();
                    ogloRxHubDBLayer = null;
                }
            }
        }

        public string validate270File_PatientInfo(string _Gender, string _City, string _State, string _Zip)
        {
            string _message = string.Empty;
            try
            {

                if (string.Compare(_Gender, "Other", true) == 0)
                {
                    return _message = "Patient gender should not be 'Other'. ";
                }

                if (_City.Length == 0)
                {
                    _message = "city";
                }

                if (_State.Length == 0)
                {
                    if (_message != string.Empty)
                    { _message = _message + ", state"; }
                    else
                    { _message = "state "; }

                }
                if (_Zip.Length == 0)
                {
                    if (_message != string.Empty)
                    { _message = _message + ", zip"; }
                    else
                    { _message = "zip"; }
                }

                 if (_message != string.Empty)
                 {
                     _message = "Patient " + _message + " should not be blank. ";
                 }
            }
            catch (Exception ex)
            {
                gloAuditTrail.gloAuditTrail.ExceptionLog(gloAuditTrail.ActivityModule.Prescription, gloAuditTrail.ActivityCategory.CreatePrescription, gloAuditTrail.ActivityType.General, ex.ToString(), gloAuditTrail.ActivityOutCome.Failure);
                throw ex;
            }

            return _message;
        }


        /// <summary>
        /// retrieve the patients eligibility 271 information 
        /// </summary>
        /// <param name="PatientID"></param>
        /// <returns></returns>
        public DataTable Get271EligibilityResponseInformation(Int64 PatientID)
        {
            string strQuery = "";
            DataTable dt_EligiblityInfo=null; //SLR: new is not neededed
            DataTable dt_datediff =null ; //SLR: new is not neededed


            clsgloRxHubDBLayer ogloRxHubDBLayer = new clsgloRxHubDBLayer();

            try
            {

                ogloRxHubDBLayer.Connect(ClsgloRxHubGeneral.ConnectionString);

                strQuery = "select TOP 1 ISNULL(dbo.gloGetDate(),NULL) AS CurrentDate ,ISNULL(DATEADD(day,-3,dbo.gloGetDate()),NULL) AS SubDate,dt270RequestDateTimeStamp from RxH_270Request_Details where spatientCode='" + PatientID + "' order by dt270RequestDateTimeStamp desc";
                dt_datediff = ogloRxHubDBLayer.ReadPatRecord(strQuery);

                ogloRxHubDBLayer.Disconnect();

                if (dt_datediff != null)
                {

                    if (dt_datediff.Rows.Count > 0)
                    {

                        dt_EligiblityInfo = ogloRxHubDBLayer.Get271ResponseDetails(PatientID, Convert.ToDateTime(dt_datediff.Rows[0]["CurrentDate"]), Convert.ToDateTime(dt_datediff.Rows[0]["SubDate"]));

                    }
                    else
                    {
                        if (dt_datediff != null)
                        {
                            dt_datediff.Dispose();
                            dt_datediff = null;
                        }
                    }

                }
                else
                {
                    if (dt_datediff != null)
                    {
                        dt_datediff.Dispose();
                        dt_datediff = null;
                    }
                }


                return dt_EligiblityInfo;


            }
            catch (Exception ex)
            {
                if (ogloRxHubDBLayer != null)
                {
                    ogloRxHubDBLayer.Disconnect();
                    ogloRxHubDBLayer.Dispose();
                    ogloRxHubDBLayer = null;
                }
                if (dt_datediff != null)
                {
                    dt_datediff.Dispose();
                    dt_datediff = null;
                }
                //SLR: This should not be disposed, since this is returned
                if (dt_EligiblityInfo != null)
                {
                    dt_EligiblityInfo.Dispose();
                    dt_EligiblityInfo = null;
                }
                gloAuditTrail.gloAuditTrail.ExceptionLog(gloAuditTrail.ActivityModule.Prescription, gloAuditTrail.ActivityCategory.CreatePrescription, gloAuditTrail.ActivityType.General, ex.ToString(), gloAuditTrail.ActivityOutCome.Failure);
                throw ex;
            }
            finally
            {

                if (ogloRxHubDBLayer != null)
                {
                    ogloRxHubDBLayer.Disconnect();
                    ogloRxHubDBLayer.Dispose();
                    ogloRxHubDBLayer = null;
                }

                if (dt_datediff != null)
                {
                    dt_datediff.Dispose();
                    dt_datediff = null;
                }
                //SLR: This should not be disposed, since this is returned
                if (dt_EligiblityInfo != null)
                {
                    //dt_EligiblityInfo.Dispose();
                    dt_EligiblityInfo = null;
                }

            }
        }


        public DataTable Get271ISAControlNumber(string ReferenceMessageId)
        {
            string strQuery = "";
            DataTable dtControlNumber = null; //SLR: new is not neededed
            
            clsgloRxHubDBLayer ogloRxHubDBLayer = new clsgloRxHubDBLayer();

            try
            {

                ogloRxHubDBLayer.Connect(ClsgloRxHubGeneral.ConnectionString);

                strQuery = "select isnull(sMessageId,'') as MsgId from dbo.RxH_271Response_Details where sReference270MessageID = '" + ReferenceMessageId + "'";
                dtControlNumber = ogloRxHubDBLayer.ReadPatRecord(strQuery);

                ogloRxHubDBLayer.Disconnect();


                return dtControlNumber;


            }
            catch (Exception ex)
            {
                if (ogloRxHubDBLayer != null)
                {
                    ogloRxHubDBLayer.Disconnect();
                    ogloRxHubDBLayer.Dispose();
                    ogloRxHubDBLayer = null;
                }
                if (dtControlNumber != null)
                {
                    dtControlNumber.Dispose();
                    dtControlNumber = null;
                }
            
                gloAuditTrail.gloAuditTrail.ExceptionLog(gloAuditTrail.ActivityModule.Prescription, gloAuditTrail.ActivityCategory.CreatePrescription, gloAuditTrail.ActivityType.General, ex.ToString(), gloAuditTrail.ActivityOutCome.Failure);
                throw ex;
            }
            finally
            {

                if (ogloRxHubDBLayer != null)
                {
                    ogloRxHubDBLayer.Disconnect();
                    ogloRxHubDBLayer.Dispose();
                    ogloRxHubDBLayer = null;
                }


            }
        }


        public DataTable Get271LatestISAControlNumberForPatient(Int64 PatientId)
        {
            string strQuery = "";
            DataTable dtControlNumber = null; //SLR: new is not neededed

            clsgloRxHubDBLayer ogloRxHubDBLayer = new clsgloRxHubDBLayer();

            try
            {

                ogloRxHubDBLayer.Connect(ClsgloRxHubGeneral.ConnectionString);

                strQuery = "select top(1) RxH_271Master.spbm_payername as payername, RxH_271Master.spbm_payerparticipantid as participantid, "
                            + " RxH_271Response_Details.smessageid as MsgId,RxH_271Response_Details.dt270ResponseDateTimeStamp as respDtTime"
                            + " FROM RxH_271Master INNER JOIN    RxH_271Response_Details  ON RxH_271Master.sMessageID = RxH_271Response_Details.sReference270messageid "
                            + "and RxH_271Response_Details.npatientid=" + PatientId + " and RxH_271Response_Details.smessageid <> '' "
                            + " order by dt270ResponseDateTimeStamp desc";
                dtControlNumber = ogloRxHubDBLayer.ReadPatRecord(strQuery);

                ogloRxHubDBLayer.Disconnect();


                return dtControlNumber;


            }
            catch (Exception ex)
            {
                if (ogloRxHubDBLayer != null)
                {
                    ogloRxHubDBLayer.Disconnect();
                    ogloRxHubDBLayer.Dispose();
                    ogloRxHubDBLayer = null;
                }
                if (dtControlNumber != null)
                {
                    dtControlNumber.Dispose();
                    dtControlNumber = null;
                }

                gloAuditTrail.gloAuditTrail.ExceptionLog(gloAuditTrail.ActivityModule.Prescription, gloAuditTrail.ActivityCategory.CreatePrescription, gloAuditTrail.ActivityType.General, ex.ToString(), gloAuditTrail.ActivityOutCome.Failure);
                throw ex;
            }
            finally
            {

                if (ogloRxHubDBLayer != null)
                {
                    ogloRxHubDBLayer.Disconnect();
                    ogloRxHubDBLayer.Dispose();
                    ogloRxHubDBLayer = null;
                }


            }
        }
        //added for ANSI 5010
        public void DeleteOld271RxhInfo(long PatientID)
        {
            clsgloRxHubDBLayer ogloRxHubDBLayer = new clsgloRxHubDBLayer();
            try
            {
                ogloRxHubDBLayer.Connect(ClsgloRxHubGeneral.ConnectionString);
                ogloRxHubDBLayer.DeleteRxH_Table("sp_DeleteRxHTables", PatientID);
                ogloRxHubDBLayer.Disconnect();
                // return true;

            }

            catch (Exception ex)
            {
                ogloRxHubDBLayer.Disconnect();
                if (ogloRxHubDBLayer != null)
                {
                    ogloRxHubDBLayer.Dispose();
                    ogloRxHubDBLayer = null;
                }

                gloAuditTrail.gloAuditTrail.ExceptionLog(gloAuditTrail.ActivityModule.Prescription, gloAuditTrail.ActivityCategory.CreatePrescription, gloAuditTrail.ActivityType.General, ex.ToString(), gloAuditTrail.ActivityOutCome.Failure);
                //return false;
            }
            finally
            {

                if (ogloRxHubDBLayer != null)
                {
                    ogloRxHubDBLayer.Disconnect();
                    ogloRxHubDBLayer.Dispose();
                    ogloRxHubDBLayer = null;
                }
            }

        }

        public DataTable GetLast270RequestDatTime(long PatientID)
        {
            clsgloRxHubDBLayer ogloRxHubDBLayer = new clsgloRxHubDBLayer();
            DataTable oDt=null ; //SLR: new is not neededed
            try
            {
                ogloRxHubDBLayer.Connect(ClsgloRxHubGeneral.ConnectionString);
                string strQuery = "SELECT dt270RequestDateTimeStamp FROM  dbo.RxH_270Request_Details where spatientcode = '586251185822738793' order by dt270RequestDateTimeStamp desc";
                oDt = ogloRxHubDBLayer.ReadPatRecord(strQuery);

                ogloRxHubDBLayer.Disconnect();

                return oDt;


            }

            catch (Exception ex)
            {
                ogloRxHubDBLayer.Disconnect();
                if (ogloRxHubDBLayer != null)
                {
                    ogloRxHubDBLayer.Dispose();
                    ogloRxHubDBLayer = null;
                }

                gloAuditTrail.gloAuditTrail.ExceptionLog(gloAuditTrail.ActivityModule.Prescription, gloAuditTrail.ActivityCategory.CreatePrescription, gloAuditTrail.ActivityType.General, ex.ToString(), gloAuditTrail.ActivityOutCome.Failure);
                return oDt;
            }
            finally
            {

                if (ogloRxHubDBLayer != null)
                {
                    ogloRxHubDBLayer.Disconnect();
                    ogloRxHubDBLayer.Dispose();
                    ogloRxHubDBLayer = null;
                }
                //SLR: SInce this is a return, don't dispose
                //if (oDt != null)
                //{
                //    oDt.Dispose();
                //    oDt = null;
                //}
            }

        }

        public DataTable GetEligiblityInformation(Int64 PatientID)
        {

            string strQuery = "";
            DataTable dt_EligiblityInfo =null ; //SLR: new is not needed
            DataTable dt_datediff =null; //SLR: new is not needed
            clsgloRxHubDBLayer ogloRxHubDBLayer = new clsgloRxHubDBLayer();
            try
            {                
                ogloRxHubDBLayer.Connect(ClsgloRxHubGeneral.ConnectionString);
                strQuery = "select TOP 1 ISNULL(dbo.gloGetDate(),NULL) AS CurrentDate ,ISNULL(DATEADD(day,-3,dbo.gloGetDate()),NULL) AS SubDate from RxH_270Request_Details";
                dt_datediff = ogloRxHubDBLayer.ReadPatRecord(strQuery);
                if (dt_datediff != null)
                {

                    if (dt_datediff.Rows.Count > 0)
                    {

                        strQuery = " SELECT  DISTINCT ISNULL(RxH_271Master.sSTLoopControlID,'') AS sSTLoopControlID,ISNULL(RxH_271Master.sPBM_PayerName,'') AS sPBM_PayerName,ISNULL(RxH_271Master.sPBM_PayerParticipantID,'') AS sPBM_PayerParticipantID, ISNULL(RxH_271Master.sPBM_PayerMemberID,'') AS sPBM_PayerMemberID, ISNULL( RxH_271Master.sPhysicianName,'') AS sPhysicianName, ISNULL( RxH_271Master.sPhysicianSuffix,'') as sPhysicianSuffix, ISNULL( RxH_271Master.sHealthPlanNumber,'') as sHealthPlanNumber, "
                        + " ISNULL(RxH_271Master.sHealthPlanName,'') as sHealthPlanName, ISNULL( RxH_271Master.sRelationshipCode,'') as sRelationshipCode , ISNULL( RxH_271Master.sRelationshipDescription,'' ) as sRelationshipDescription, ISNULL( RxH_271Master.sPersonCode,'') as sPersonCode, ISNULL(RxH_271Master.sCardHolderID,'') as sCardHolderID, ISNULL( RxH_271Master.sGroupID,'') as sGroupID, ISNULL( RxH_271Master.sCardHolderName,'') as sCardHolderName, ISNULL( RxH_271Master.sGroupName,'') as sGroupName, "
                        + " ISNULL(RxH_271Master.sFormularyListID,'') as sFormularyListID, ISNULL( RxH_271Master.sAlternativeListID,'') as sAlternativeListID, ISNULL( RxH_271Master.sCoverageID,'') as sCoverageID, ISNULL( RxH_271Master.sEmployeeID,'') as sEmployeeID, ISNULL( RxH_271Master.sBINNumber,'') as sBINNumber, ISNULL(RxH_271Master.sCoPayID,'') as sCoPayID, ISNULL( RxH_271Master.sPharmacyEligible,'') as sPharmacyEligible, ISNULL( RxH_271Master.sPharmacyCoverageName,'') as sPharmacyCoverageName, ISNULL(RxH_271Master.sPhEligiblityorBenefitInfo,'') as sPhEligiblityorBenefitInfo, ISNULL( RxH_271Master.sMailOrderRxDrugEligible,'') as sMailOrderRxDrugEligible, ISNULL( RxH_271Master.sMailOrderRxDrugCoverageName,'') as sMailOrderRxDrugCoverageName, "
                        + " ISNULL(RxH_271Master.sMailOrdEligiblityorBenefitInfo,'') as sMailOrdEligiblityorBenefitInfo, ISNULL(RxH_271Master.sMailOrderInsTypeCode,'') as sMailOrderInsTypeCode,ISNULL(RxH_271Master.sRetailInsTypeCode,'') as sRetailInsTypeCode, ISNULL( RxH_271Master.sEligiblityDate,'') as sEligiblityDate,ISNULL( RxH_271Master.sServiceDate,'') as sServiceDate, ISNULL(RxH_271Master.sMailOrderMonetaryAmount,'') as sMailOrderMonetaryAmount, ISNULL(RxH_271Master.sRetailMonetaryAmount,'') as sRetailMonetaryAmount,"
                        + " (ISNULL(RxH_271Master.sDFirstName,'') + ' '+ ISNULL(RxH_271Master.sDMiddleName,'') + ' ' + ISNULL( RxH_271Master.sDLastName,'')) AS DependentName, ISNULL(RxH_271Master.sDGender,'') as sDGender, ISNULL( RxH_271Master.sDDOB,'') as sDDOB, ISNULL( RxH_271Master.sDSSN,'') as sDSSN, ISNULL( RxH_271Master.sDAddress1,'') as sDAddress1, ISNULL( RxH_271Master.sDAddress2,'') as sDAddress2, ISNULL(RxH_271Master.sDCity,'') as sDCity, ISNULL( RxH_271Master.sDState,'') as sDState, ISNULL( RxH_271Master.sDZip,'') as sDZip , ISNULL( RxH_271Master.nPatientID,0) as nPatientID, ISNULL( RxH_271Master.dtResquestDateTimeStamp,'') as dtResquestDateTimeStamp, "
                        + " ISNULL(RxH_271Master.sMessageID,'') as sMessageID,ISNULL(RxH_271Master.IsDependentdemoChange,'false') AS IsDependentdemoChange,(ISNULL(RxH_271Master.sDependentdemochgFirstName,'') + ' ' + ISNULL(RxH_271Master.sDependentdemoChgMiddleName,'') + ' ' + ISNULL(RxH_271Master.sDependentdemoChgLastName,'')) AS DependentdemoChgName,ISNULL(RxH_271Master.sDependentdemoChgGender,'') AS sDependentdemoChgGender,ISNULL(RxH_271Master.sDependentdemoChgDOB,'') AS sDependentdemoChgDOB,ISNULL(RxH_271Master.sDependentdemoChgSSN,'') AS sDependentdemoChgSSN, "
                        + " ISNULL(RxH_271Master.sDependentdemoChgAddress1,'') AS sDependentdemoChgAddress1,ISNULL(RxH_271Master.sDependentdemoChgAddress2,'') AS sDependentdemoChgAddress2,ISNULL(RxH_271Master.sDependentdemoChgCity,'') AS sDependentdemoChgCity,ISNULL(RxH_271Master.sDependentdemoChgState,'') AS sDependentdemoChgState,ISNULL(RxH_271Master.sDependentdemoChgZip,'') AS sDependentdemoChgZip,(ISNULL(RxH_271Details.sSubscriberFirstName,'') + ' ' + ISNULL( RxH_271Details.sSubscriberMiddleName,'') + ' ' + ISNULL(RxH_271Details.sSubscriberLastName,'')) AS SubscriberName, ISNULL(RxH_271Details.sSubscriberSuffix,'') as SubscriberSuffix,ISNULL( RxH_271Details.sSubscriberGender,'') as sSubscriberGender, ISNULL( RxH_271Details.sSubscriberDOB,'') as sSubscriberDOB, ISNULL( RxH_271Details.sSubscriberSSN,'' ) as sSubscriberSSN,  ISNULL(RxH_271Details.sSubscriberAddress1,'') as sSubscriberAddress1, ISNULL( RxH_271Details.sSubscriberAddress2,'') as sSubscriberAddress2, ISNULL( RxH_271Details.sSubscriberCity,'') as sSubscriberCity, ISNULL( RxH_271Details.sSubscriberState,'') as sSubscriberState, ISNULL(RxH_271Details.sSubscriberZip,'') as sSubscriberZip, "
                        + " ISNULL(RxH_271Details.IsSubscriberdemoChange,'false') AS IsSubscriberdemoChange,(ISNULL(RxH_271Details.sSubscriberDemochgFirstName,'') + ' ' + ISNULL(RxH_271Details.sSubscriberDemoChgMiddleName,'') + ' ' + ISNULL(RxH_271Details.sSubscriberDemoChgLastName,'')) AS SubscriberDemoChgName,ISNULL(RxH_271Details.sSubscriberDemoChgGender,'') AS sSubscriberDemoChgGender,ISNULL(RxH_271Details.sSubscriberDemoChgDOB,'') AS sSubscriberDemoChgDOB,ISNULL(RxH_271Details.sSubscriberDemoChgSSN,'') AS sSubscriberDemoChgSSN,ISNULL(RxH_271Details.sSubscriberDemoChgAddress1,'') AS sSubscriberDemoChgAddress1,ISNULL(RxH_271Details.sSubscriberDemoChgAddress2,'') AS sSubscriberDemoChgAddress2, "
                        + " ISNULL(RxH_271Details.sSubscriberDemoChgCity,'') AS sSubscriberDemoChgCity,ISNULL(RxH_271Details.sSubscriberDemoChgState,'') AS sSubscriberDemoChgState,ISNULL(RxH_271Details.sSubscriberDemoChgZip,'') AS sSubscriberDemoChgZip, "
                        + " ISNULL(RxH_271Master.sIsContractedProvider,'') AS sIsContractedProvider,ISNULL(RxH_271Master.sContractedProviderName,'') AS sContractedProviderName,ISNULL(RxH_271Master.sContractedProviderNumber,'') AS sContractedProviderNumber,ISNULL(RxH_271Master.sContProvMailOrderEligible,'') AS	sContProvMailOrderEligible,ISNULL(RxH_271Master.sContProvMailOrderCoverageInfo,'') AS	sContProvMailOrderCoverageInfo,ISNULL(RxH_271Master.sContProvMailOrderInsTypeCode,'') AS sContProvMailOrderInsTypeCode,ISNULL(RxH_271Master.sContProvMailOrderMonetaryAmt,'') AS sContProvMailOrderMonetaryAmt,ISNULL(RxH_271Master.sContProvRetailsEligible,'') AS sContProvRetailsEligible,ISNULL(RxH_271Master.sContProvRetailCoverageInfo,'') AS sContProvRetailCoverageInfo,	ISNULL(RxH_271Master.sContProvRetailInsTypeCode,'') AS	sContProvRetailInsTypeCode,ISNULL(RxH_271Master.sContProvRetailMonetaryAmt,'') AS sContProvRetailMonetaryAmt,ISNULL(RxH_271Master.sIsPrimaryPayer,'') AS sIsPrimaryPayer,ISNULL(RxH_271Master.sPrimaryPayerName,'') AS sPrimaryPayerName,	ISNULL(RxH_271Master.sPrimaryPayerNumber,'') AS sPrimaryPayerNumber, "
                        + " ISNULL(RxH_271Master.sPrimaryPayerMailOrderEligible,'') AS	sPrimaryPayerMailOrderEligible,ISNULL(RxH_271Master.sPrimaryPayerMailOrderCoverageInfo,'') AS	sPrimaryPayerMailOrderCoverageInfo,ISNULL(RxH_271Master.sPrimaryPayerMailOrderInsTypeCode	,'') AS	sPrimaryPayerMailOrderInsTypeCode,ISNULL(RxH_271Master.sPrimaryPayerMailOrderMonetaryAmt,'') AS sPrimaryPayerMailOrderMonetaryAmt,	ISNULL(RxH_271Master.sPrimaryPayerRetailsEligible,'') AS sPrimaryPayerRetailsEligible,ISNULL(RxH_271Master.sPrimaryPayerRetailCoverageInfo,'') AS sPrimaryPayerRetailCoverageInfo,		ISNULL(RxH_271Master.sPrimaryPayerRetailInsTypeCode,'') AS	sPrimaryPayerRetailInsTypeCode,ISNULL(RxH_271Master.sPrimaryPayerRetailMonetaryAmt,'') AS	sPrimaryPayerRetailMonetaryAmt , isnull(RxH_271Response_Details.sMessageType,'') as sMessageType, isnull(RxH_271Response_Details.sPBM_PayerParticipantID,'') AS Resp271_PBMPArticipantID "
                        + " FROM RxH_271Master INNER JOIN "
                        + " RxH_271Details ON RxH_271Master.sMessageID = RxH_271Details.sMessageID AND RxH_271Details.sSTLoopControlID = RxH_271Master.sSTLoopControlID INNER JOIN "
                        + " RxH_271Response_Details ON RxH_271Master.sPBM_PayerParticipantID = RxH_271Response_Details.sPBM_PayerParticipantID and RxH_271Master.sMessageID = RxH_271Response_Details.sReference270MessageID"
                        + " WHERE RxH_271Master.nPatientID = " + PatientID + " "
                        + " AND dtResquestDateTimeStamp between  '" + dt_datediff.Rows[0]["SubDate"] + "' and '" + dt_datediff.Rows[0]["CurrentDate"] + "' Order by dtResquestDateTimeStamp desc";
                        //+ "AND convert(datetime,convert(varchar,dtResquestDateTimeStamp,101)) = '" + DateTime.Now.ToShortDateString() + "' Order by dtResquestDateTimeStamp desc";
                        dt_EligiblityInfo = ogloRxHubDBLayer.ReadPatRecord(strQuery);
                    }

                }


                return dt_EligiblityInfo;

            }
            catch (Exception ex)
            {
                gloAuditTrail.gloAuditTrail.ExceptionLog(gloAuditTrail.ActivityModule.Prescription, gloAuditTrail.ActivityCategory.CreatePrescription, gloAuditTrail.ActivityType.General, ex.ToString(), gloAuditTrail.ActivityOutCome.Failure);
                return dt_EligiblityInfo;
            }
            finally
            {
                //SLR: dispose dt_datediff, disconnect and dispose ogloRxHuBDBLayr
                if (dt_datediff != null)
                {
                    dt_datediff.Dispose();
                    dt_datediff = null;
                }
                if (ogloRxHubDBLayer != null)
                {
                    ogloRxHubDBLayer.Disconnect();
                    ogloRxHubDBLayer.Dispose();
                    ogloRxHubDBLayer = null;
                }
            }

        }
        
        static ediDocument myEdiDoc = null;
        static ediSchemas myEdiSchemas = null;
        public static ediSchemas myEDISchemaObject(int options, ref ediSchemas reAssignEdiSchemaObject)
        {
            if (options == 0)
            {
                if (myEdiSchemas != null)
                {
                    myEdiSchemas.Dispose();
                    myEdiSchemas = null;
                  
                                    }
            }
            if (options == 2)
            {
                myEdiSchemas = reAssignEdiSchemaObject;
            }
            return myEdiSchemas;
        }

        public static ediDocument myEDIObject(int options, ref ediDocument reAssignEdiObject )
        {
            if (options == 0)
            {
                if (myEdiDoc != null)
                {
                    //try
                    //{

                    //    oEdiDoc.Close();
                    //}
                    //catch (Exception ex)
                    //{

                    //}
                    try
                    {
                        try
                        {
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(myEdiDoc);
                            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(myEdiDoc);
                        }
                        catch //(Exception ex1)
                        {

                        }
                        try
                        {
                            myEdiDoc.Dispose();
                        }
                        catch //(Exception ex3)
                        {
                        }
                    }
                    catch //(Exception ex2)
                    {

                    }

                    myEdiDoc = null;

                }
            }
            if (options == 2)
            {
                myEdiDoc = reAssignEdiObject;
            }
            return myEdiDoc;
        }


        public bool IsEligibilitygGenerated_validation(Int64 PatientID, String RxEligThresholdval)
        {
            //string _s270ReqResponse_details = "";
            string strQuery = "";
            System.Data.DataTable dt_Reqtable=null ; //SLR: new is not needed
            System.Data.DataTable dt_271Respdettable=null ; //SLR: new is not needed
            clsgloRxHubDBLayer ogloRxHubDBLayer = new clsgloRxHubDBLayer();
            bool Result = false;
       //     bool _ContainPNF = true;
            string _RxEligThresholdval = RxEligThresholdval;
            //// If Result is TRUE then send resquest
            //// IF Result is false then dont send Request
            try
            {
                ogloRxHubDBLayer.Connect(ClsgloRxHubGeneral.ConnectionString);
                //Previously in 4010 we had 24 hours validation
                //strQuery = "select top 1 *, ISNULL(dt270RequestDateTimeStamp,NULL) AS LastDate ,ISNULL(DATEADD(day,1,dt270RequestDateTimeStamp),NULL) AS AddedDate from RxH_270Request_Details where sPatientCode= '" + PatientID + "' order by dt270RequestDateTimeStamp desc ";
                //from 5010 we have 72 hours (3 Days) validation
                strQuery = "select top 1 *, ISNULL(dt270RequestDateTimeStamp,NULL) AS LastDate ,ISNULL(DATEADD(day," + _RxEligThresholdval + ",dt270RequestDateTimeStamp),NULL) AS AddedDate from RxH_270Request_Details where sPatientCode= '" + PatientID + "' order by dt270RequestDateTimeStamp desc ";
                dt_Reqtable = ogloRxHubDBLayer.ReadPatRecord(strQuery);
                ogloRxHubDBLayer.Disconnect();
                if (dt_Reqtable != null)
                {

                    if (dt_Reqtable.Rows.Count > 0)
                    {
                        string _271MsgTransactionID = dt_Reqtable.Rows[0]["sTransactionID"].ToString();

                        /// this prop proc variable was especially created to show the Last Eligibility Request Datetime in message box, when the eligibility for patient is expired and user tries to get medication history
                        /// code implemented to resolve residual bug # 42438 added in 7020 
                        _dtLastEligibilityRequestDatetime = dt_Reqtable.Rows[0]["LastDate"].ToString();

                        //compare this  transaction ID in 271_reponse_details table and check the message type column.
                        //if the message type contains message type like "271|PNF" this means that the "patient was not found"
                        //therefor user is eligible to send the 270 request again.
                        ogloRxHubDBLayer.Connect(ClsgloRxHubGeneral.ConnectionString);
                        strQuery = "select * from  RxH_271Response_Details where sreference270messageid = '" + _271MsgTransactionID + "'";
                        dt_271Respdettable = ogloRxHubDBLayer.ReadPatRecord(strQuery);
                        ogloRxHubDBLayer.Disconnect();
                        if (dt_271Respdettable != null)
                        {
                            if (dt_271Respdettable.Rows.Count > 0)
                            {
                                for (int i = 0; i <= dt_271Respdettable.Rows.Count - 1; i++)
                                {
                                    string _MessageType = dt_271Respdettable.Rows[i]["sMessageType"].ToString();

                                    if (_MessageType != "")
                                    {
                                        if (_MessageType.Contains("|"))
                                        {
                                            string sRejectionCodeDesc = "";
                                            string[] sRejectCd = null;
                                            sRejectCd = Microsoft.VisualBasic.Strings.Split(_MessageType, "|", 3, Microsoft.VisualBasic.CompareMethod.Text); ;
                                            if (sRejectCd.Length >= 2)
                                            {
                                                sRejectionCodeDesc = getRejecttionDescription(sRejectCd[1].ToString(), "", "");
                                            }                                              
                                            sRejectCd = null;

                                            //Delete the RxH_270Request_Details/RxH_271Details/RxH_271Master/RxH_271Response_Details table 
                                            //for the entries made against this patient
                                            if (sRejectionCodeDesc != "")
                                            {
                                                ogloRxHubDBLayer.Connect(ClsgloRxHubGeneral.ConnectionString);
                                                ogloRxHubDBLayer.DeleteRxH_Table("gsp_DeleteRxHTables", Convert.ToInt64(dt_271Respdettable.Rows[i]["npatientid"]));
                                                ogloRxHubDBLayer.Disconnect();
                                            }
                                         //   _ContainPNF = true;
                                            Result = true;
                                            return Result;
                                        }
                                        else
                                        {
                                        //    _ContainPNF = false;
                                            ogloRxHubDBLayer.Connect(ClsgloRxHubGeneral.ConnectionString);
                                            Result = ogloRxHubDBLayer.GetReuestDetails(Convert.ToDateTime(dt_Reqtable.Rows[0]["LastDate"]), Convert.ToDateTime(dt_Reqtable.Rows[0]["AddedDate"]));
                                            ogloRxHubDBLayer.Disconnect();
                                            //Result = false;
                                            return Result;
                                        }
                                    }

                                }

                            }
                            else//this means there is no response returned agains that file or it was a NAK file
                            {
                                Result = true;
                            }
                        }



                    }
                    else
                    {
                        Result = true;
                    }

                }
                else
                {
                    Result = true;
                }
            }
            catch (Exception ex)
            {
                gloAuditTrail.gloAuditTrail.ExceptionLog(gloAuditTrail.ActivityModule.Prescription, gloAuditTrail.ActivityCategory.CreatePrescription, gloAuditTrail.ActivityType.General, ex.ToString(), gloAuditTrail.ActivityOutCome.Failure);
                return false;
            }
            finally
            {
                //SLR: Free dtreqtable, dt271_respdettable, ogloRxHubDBlayer, 
                if (ogloRxHubDBLayer != null)
                {
                    ogloRxHubDBLayer.Disconnect();
                    ogloRxHubDBLayer.Dispose();
                    ogloRxHubDBLayer = null;
                }
                if (dt_Reqtable != null)
                {
                    dt_Reqtable.Dispose();
                    dt_Reqtable = null;
                }
                if (dt_271Respdettable != null)
                {
                    dt_271Respdettable.Dispose();
                    dt_271Respdettable = null;
                }
            }

            return Result;

        }
        //TODO: Need To Refactor // Used To Check Eligibility Status
        //public RxBusinesslayer.EligibilityStatus GetEligibilityStatus(Int64 PatientID, String RxEligThresholdval)
        //{
        //    string strQuery = "";
        //    System.Data.DataTable dt_Reqtable = null;
        //    System.Data.DataTable dt_271Respdettable = null; 
        //    clsgloRxHubDBLayer ogloRxHubDBLayer = new clsgloRxHubDBLayer();
        //    RxBusinesslayer.EligibilityStatus Result = RxBusinesslayer.EligibilityStatus.NotChecked;          
                   
        //    try
        //    {
        //        ogloRxHubDBLayer.Connect(ClsgloRxHubGeneral.ConnectionString);
        //        strQuery = "select top 1 *, ISNULL(dt270RequestDateTimeStamp,NULL) AS LastDate ,ISNULL(DATEADD(day," + RxEligThresholdval + ",dt270RequestDateTimeStamp),NULL) AS AddedDate from RxH_270Request_Details where sPatientCode= '" + PatientID + "' order by dt270RequestDateTimeStamp desc ";
        //        dt_Reqtable = ogloRxHubDBLayer.ReadPatRecord(strQuery);
        //        ogloRxHubDBLayer.Disconnect();
        //        if (dt_Reqtable != null)
        //        {
        //            if (dt_Reqtable.Rows.Count > 0)
        //            {
        //                string _271MsgTransactionID = dt_Reqtable.Rows[0]["sTransactionID"].ToString();
        //                _dtLastEligibilityRequestDatetime = dt_Reqtable.Rows[0]["LastDate"].ToString();
        //                ogloRxHubDBLayer.Connect(ClsgloRxHubGeneral.ConnectionString);
        //                strQuery = "select * from  RxH_271Response_Details where sreference270messageid = '" + _271MsgTransactionID + "'";
        //                dt_271Respdettable = ogloRxHubDBLayer.ReadPatRecord(strQuery);
        //                ogloRxHubDBLayer.Disconnect();
        //                if (dt_271Respdettable != null)
        //                {
        //                    if (dt_271Respdettable.Rows.Count > 0)
        //                    {
        //                        for (int i = 0; i <= dt_271Respdettable.Rows.Count - 1; i++)
        //                        {
        //                            string _MessageType = dt_271Respdettable.Rows[i]["sMessageType"].ToString();

        //                            if (_MessageType != "")
        //                            {
        //                                if (_MessageType.Contains("|"))
        //                                {
        //                                    string sRejectionCodeDesc = "";
        //                                    string[] sRejectCd = null;
        //                                    sRejectCd = Microsoft.VisualBasic.Strings.Split(_MessageType, "|", 3, Microsoft.VisualBasic.CompareMethod.Text); ;
        //                                    if (sRejectCd.Length >= 2)
        //                                    {
        //                                        sRejectionCodeDesc = getRejecttionDescription(sRejectCd[1].ToString(), "", "");
        //                                    }
        //                                    sRejectCd = null;
        //                                    Result = RxBusinesslayer.EligibilityStatus.Failed;
        //                                    return Result;
        //                                }
        //                                else
        //                                {
        //                                    ogloRxHubDBLayer.Connect(ClsgloRxHubGeneral.ConnectionString);
        //                                    if (ogloRxHubDBLayer.GetReuestDetails(Convert.ToDateTime(dt_Reqtable.Rows[0]["LastDate"]), Convert.ToDateTime(dt_Reqtable.Rows[0]["AddedDate"])) == true)
        //                                    { Result = RxBusinesslayer.EligibilityStatus.NotChecked;}
        //                                    else
        //                                    { Result = RxBusinesslayer.EligibilityStatus.Passed; }
        //                                    ogloRxHubDBLayer.Disconnect();
        //                                    return Result;
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        gloAuditTrail.gloAuditTrail.ExceptionLog(gloAuditTrail.ActivityModule.Prescription, gloAuditTrail.ActivityCategory.Eligibility, gloAuditTrail.ActivityType.General, ex.ToString(), gloAuditTrail.ActivityOutCome.Failure);
        //    }
        //    finally
        //    {
        //        if (ogloRxHubDBLayer != null)
        //        {
        //            ogloRxHubDBLayer.Disconnect();
        //            ogloRxHubDBLayer.Dispose();
        //            ogloRxHubDBLayer = null;
        //        }
        //        if (dt_Reqtable != null)
        //        {
        //            dt_Reqtable.Dispose();
        //            dt_Reqtable = null;
        //        }
        //        if (dt_271Respdettable != null)
        //        {
        //            dt_271Respdettable.Dispose();
        //            dt_271Respdettable = null;
        //        }
        //    }
        //    return Result;
        //}#region MyRegion
		 
        public RxBusinesslayer.EligibilityStatus GetEligibilityStatus(Int64 PatientID, String RxEligThresholdval)
        {
            string strQuery = "";
            System.Data.DataTable dt_Reqtable = null;
            clsgloRxHubDBLayer ogloRxHubDBLayer = new clsgloRxHubDBLayer();
            RxBusinesslayer.EligibilityStatus Result = RxBusinesslayer.EligibilityStatus.NotChecked;

            try
            {
                ogloRxHubDBLayer.Connect(ClsgloRxHubGeneral.ConnectionString);
                strQuery = "select top 1 *, ISNULL(dt270RequestDateTimeStamp,NULL) AS LastDate ,ISNULL(DATEADD(day," + RxEligThresholdval + ",dt270RequestDateTimeStamp),NULL) AS AddedDate from RxH_270Request_Details where sPatientCode= '" + PatientID + "' order by dt270RequestDateTimeStamp desc ";
                dt_Reqtable = ogloRxHubDBLayer.ReadPatRecord(strQuery);
                ogloRxHubDBLayer.Disconnect();
                if (dt_Reqtable != null)
                {
                    if (dt_Reqtable.Rows.Count > 0)
                    {
                        ogloRxHubDBLayer.Connect(ClsgloRxHubGeneral.ConnectionString);
                        if (ogloRxHubDBLayer.GetReuestDetails(Convert.ToDateTime(dt_Reqtable.Rows[0]["LastDate"]), Convert.ToDateTime(dt_Reqtable.Rows[0]["AddedDate"])) == true)
                        { Result = RxBusinesslayer.EligibilityStatus.NotChecked; }
                        else
                        {
                            if (ogloRxHubDBLayer.ISPBMExist(PatientID) != true)
                            { Result = RxBusinesslayer.EligibilityStatus.Failed; }
                            else
                            { Result = RxBusinesslayer.EligibilityStatus.Passed; }
                        }
                        ogloRxHubDBLayer.Disconnect();
                        return Result;
                    }
                }
            }
            catch (Exception ex)
            {
                gloAuditTrail.gloAuditTrail.ExceptionLog(gloAuditTrail.ActivityModule.Prescription, gloAuditTrail.ActivityCategory.Eligibility, gloAuditTrail.ActivityType.General, ex.ToString(), gloAuditTrail.ActivityOutCome.Failure);
            }
            finally
            {
                if (ogloRxHubDBLayer != null)
                {
                    ogloRxHubDBLayer.Disconnect();
                    ogloRxHubDBLayer.Dispose();
                    ogloRxHubDBLayer = null;
                }
                if (dt_Reqtable != null)
                {
                    dt_Reqtable.Dispose();
                    dt_Reqtable = null;
                }                
            }
            return Result;
        }

	
        
        public bool SS10dot6IsEligibilitygGenerated_validation(Int64 PatientID, String RxEligThresholdval)
        {
            //string _s270ReqResponse_details = "";
            string strQuery = "";
            System.Data.DataTable dt_Reqtable = null; //SLR: new is not needed
            System.Data.DataTable dt_271Respdettable = null; //SLR: new is not needed
            clsgloRxHubDBLayer ogloRxHubDBLayer = new clsgloRxHubDBLayer();
            bool Result = false;
         //   bool _ContainPNF = true;
            string _RxEligThresholdval = RxEligThresholdval;
            //// If Result is TRUE then send resquest
            //// IF Result is false then dont send Request
            try
            {
                ogloRxHubDBLayer.Connect(ClsgloRxHubGeneral.ConnectionString);
                //Previously in 4010 we had 24 hours validation
                //strQuery = "select top 1 *, ISNULL(dt270RequestDateTimeStamp,NULL) AS LastDate ,ISNULL(DATEADD(day,1,dt270RequestDateTimeStamp),NULL) AS AddedDate from RxH_270Request_Details where sPatientCode= '" + PatientID + "' order by dt270RequestDateTimeStamp desc ";
                //from 5010 we have 72 hours (3 Days) validation
                strQuery = "select top 1 *, ISNULL(dt270RequestDateTimeStamp,NULL) AS LastDate ,ISNULL(DATEADD(day," + _RxEligThresholdval + ",dt270RequestDateTimeStamp),NULL) AS AddedDate from RxH_270Request_Details where sPatientCode= '" + PatientID + "' order by dt270RequestDateTimeStamp desc ";
                dt_Reqtable = ogloRxHubDBLayer.ReadPatRecord(strQuery);
                ogloRxHubDBLayer.Disconnect();
                if (dt_Reqtable != null)
                {

                    if (dt_Reqtable.Rows.Count > 0)
                    {
                        string _271MsgTransactionID = dt_Reqtable.Rows[0]["sTransactionID"].ToString();

                        /// this prop proc variable was especially created to show the Last Eligibility Request Datetime in message box, when the eligibility for patient is expired and user tries to get medication history
                        /// code implemented to resolve residual bug # 42438 added in 7020 
                        _dtLastEligibilityRequestDatetime = dt_Reqtable.Rows[0]["LastDate"].ToString();

                        //compare this  transaction ID in 271_reponse_details table and check the message type column.
                        //if the message type contains message type like "271|PNF" this means that the "patient was not found"
                        //therefor user is eligible to send the 270 request again.
                        ogloRxHubDBLayer.Connect(ClsgloRxHubGeneral.ConnectionString);
                        strQuery = "select * from  RxH_271Response_Details where sreference270messageid = '" + _271MsgTransactionID + "'";
                        dt_271Respdettable = ogloRxHubDBLayer.ReadPatRecord(strQuery);
                        ogloRxHubDBLayer.Disconnect();
                        if (dt_271Respdettable != null)
                        {
                            if (dt_271Respdettable.Rows.Count > 0)
                            {
                                for (int i = 0; i <= dt_271Respdettable.Rows.Count - 1; i++)
                                {
                                    string _MessageType = dt_271Respdettable.Rows[i]["sMessageType"].ToString();

                                    if (_MessageType != "")
                                    {
                                        if (_MessageType.Contains("|"))
                                        {
                                            string sRejectionCodeDesc = "";
                                            string[] sRejectCd = null;
                                            sRejectCd = Microsoft.VisualBasic.Strings.Split(_MessageType, "|", 3, Microsoft.VisualBasic.CompareMethod.Text); ;
                                            if (sRejectCd.Length >= 2)
                                            {
                                                sRejectionCodeDesc = getRejecttionDescription(sRejectCd[1].ToString(), "", "");
                                            }
                                            sRejectCd = null;

                                            //Delete the RxH_270Request_Details/RxH_271Details/RxH_271Master/RxH_271Response_Details table 
                                            //for the entries made against this patient
                                            if (sRejectionCodeDesc != "")
                                            {
                                                ogloRxHubDBLayer.Connect(ClsgloRxHubGeneral.ConnectionString);
                                                //ogloRxHubDBLayer.DeleteRxH_Table("gsp_DeleteRxHTables", Convert.ToInt64(dt_271Respdettable.Rows[i]["npatientid"]));
                                                ogloRxHubDBLayer.Disconnect();
                                            }
                                        //    _ContainPNF = true;
                                            Result = true;
                                            return Result;
                                        }
                                        else
                                        {
                                         //   _ContainPNF = false;
                                            ogloRxHubDBLayer.Connect(ClsgloRxHubGeneral.ConnectionString);
                                            Result = ogloRxHubDBLayer.GetReuestDetails(Convert.ToDateTime(dt_Reqtable.Rows[0]["LastDate"]), Convert.ToDateTime(dt_Reqtable.Rows[0]["AddedDate"]));
                                            ogloRxHubDBLayer.Disconnect();
                                        }
                                    }

                                }

                            }
                            else//this means there is no response returned agains that file or it was a NAK file
                            {
                                Result = true;
                            }
                        }



                    }
                    else
                    {
                        Result = true;
                    }

                }
                else
                {
                    Result = true;
                }
            }
            catch (Exception ex)
            {
                gloAuditTrail.gloAuditTrail.ExceptionLog(gloAuditTrail.ActivityModule.Prescription, gloAuditTrail.ActivityCategory.CreatePrescription, gloAuditTrail.ActivityType.General, ex.ToString(), gloAuditTrail.ActivityOutCome.Failure);
                return false;
            }
            finally
            {
                //SLR: Free dtreqtable, dt271_respdettable, ogloRxHubDBlayer, 
                if (ogloRxHubDBLayer != null)
                {
                    ogloRxHubDBLayer.Disconnect();
                    ogloRxHubDBLayer.Dispose();
                    ogloRxHubDBLayer = null;
                }
                if (dt_Reqtable != null)
                {
                    dt_Reqtable.Dispose();
                    dt_Reqtable = null;
                }
                if (dt_271Respdettable != null)
                {
                    dt_271Respdettable.Dispose();
                    dt_271Respdettable = null;
                }
            }

            return Result;

        }
        public bool LoadEDIObject_271_5010(string _271FilePath)
        {
            ediDocument dummy = null;
            ediSchemas dummySchemas = null;
            ediDocument oEdiDoc = myEDIObject(0, ref dummy);
            ediSchemas oSchemas = myEDISchemaObject(0, ref dummySchemas);
            //ediAcknowledgment oAck = null;
            try
            {
                //SLR: Free previously allocated incident before allocating once more new
                FreeEDIObject(ref oEdiDoc);
                //oEdiDoc = new ediDocument();
                ediDocument.Set(ref oEdiDoc, new ediDocument());
                myEDIObject(2, ref oEdiDoc);
                // oEdiDoc = new ediDocument();  // SLR: When is this new freed. Please read guidline oEdiDoc = new ediDocument();
                sPath = AppDomain.CurrentDomain.BaseDirectory;
                sSEFFile = "271_005010X279A1.SemRef.SEF";//"271_005010X279A1.SemRef.SEF" ;//271_X092A1.SEF//271_005010X279.SemRef.SEF
                sEdiFile = "271.X12";

                // Disabling the internal standard reference library to makes sure that 
                ediSchemas.Set(ref oSchemas, (ediSchemas)oEdiDoc.GetSchemas());    //oSchemas = (ediSchemas) oEdiDoc.GetSchemas();
                myEDISchemaObject(2, ref oSchemas);
                oSchemas.EnableStandardReference = false;

                // This makes certain that the EDI file must use the same version SEF file, otherwise
                // the process will stop.
                oSchemas.set_Option(SchemasOptionIDConstants.OptSchemas_VersionRestrict, 1);

                // By setting the cursor type to ForwardOnly, FREDI does not load the entire file into memory, which
                // improves performance when processing larger EDI files.
                oEdiDoc.CursorType = DocumentCursorTypeConstants.Cursor_ForwardOnly;


                oEdiDoc.LoadSchema(sPath + sSEFFile, 0);
                //oEdiDoc.LoadSchema(sPath + "997_X12-4010.SEF", 0); //Commented while doing ANSI 5010


                //oEdiDoc.LoadEdi(sPath + sEdiFile);
                oEdiDoc.LoadEdi(_271FilePath);

                return true;
            }
            catch (Exception ex)
            {
                gloAuditTrail.gloAuditTrail.ExceptionLog(gloAuditTrail.ActivityModule.Prescription, gloAuditTrail.ActivityCategory.CreatePrescription, gloAuditTrail.ActivityType.General, ex.ToString(), gloAuditTrail.ActivityOutCome.Failure);
                return false;
                throw ex;


            }
            //SLR: Finally free oEDIdoc? or leave the memory to be freed at next time according to the implemntationof memory
        }


        public string PostEDIFile_5010(string strfilename,string RequestType)
        {
            string strOutputfilename = "";
            try
            {
                strOutputfilename = ExtractText(ConvertFiletoBinaryEDI_5010(strfilename, RequestType));
                return strOutputfilename;
            }
            catch (Exception ex)
            {
                gloAuditTrail.gloAuditTrail.ExceptionLog(gloAuditTrail.ActivityModule.Prescription, gloAuditTrail.ActivityCategory.CreatePrescription, gloAuditTrail.ActivityType.General, ex.ToString(), gloAuditTrail.ActivityOutCome.Failure);
                return strOutputfilename;

            }

        }

        internal byte[] ConvertFiletoBinaryEDI_5010(string strFileName, string RequestType)
        {
            if (File.Exists(strFileName))
            {
                FileStream oFile = default(FileStream);
                BinaryReader oReader = default(BinaryReader);
                byte[] readbytes = null;
                DataSet ds = new DataSet();
                try
                {
                    using (gloDatabaseLayer.DBLayer oDB = new gloDatabaseLayer.DBLayer(ClsgloRxHubGeneral.ConnectionString))
                    {
                        oDB.Connect(false);
                        oDB.Retrive_Query("SELECT ISNULL(sSettingsValue,'') AS sSettingsValue FROM Settings WITH (NOLOCK) WHERE UPPER(sSettingsName) in ('eRxStagingWebserviceURL','eRxProductionWebserviceURL','eRx10dot6ProductionWebserviceURL','eRx10dot6StagingWebserviceURL') order by sSettingsName", out ds);
                        oDB.Disconnect();
                    }

                    oFile = new FileStream(strFileName, FileMode.Open, FileAccess.Read);

                    oReader = new BinaryReader(oFile);

                    byte[] bytesRead = oReader.ReadBytes(Convert.ToInt32(oFile.Length));

                    string sUriString = string.Empty;

                    if (ClsgloRxHubGeneral.gblnIsRxhubStagingServer == true)
                    {
                        sUriString = ds.Tables[0].Rows[1]["sSettingsValue"].ToString();
                    }
                    else
                    { 
                        sUriString = ds.Tables[0].Rows[0]["sSettingsValue"].ToString();
                    }

                    WCFRxElignMedHx.IeRxClient client = null;
                    Uri serviceUri = null;
                    System.ServiceModel.EndpointAddress endpointAddress = null;
                    WSHttpBinding binding = null;
                    try
                    {
                        serviceUri = new Uri(sUriString);
                        endpointAddress = new System.ServiceModel.EndpointAddress(serviceUri);

                        binding = BindingFactory.CreateInstance();
                        client = new WCFRxElignMedHx.IeRxClient(binding, endpointAddress);

                        readbytes = client.PostClient270nMedHxMessage(bytesRead, RequestType);

                        client.Close();
                        serviceUri = null;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        if ((binding != null))
                        {
                            binding = null;
                        }
                        if ((endpointAddress != null))
                        {
                            endpointAddress = null;
                        }
                        if ((serviceUri != null))
                        {
                            serviceUri = null;
                        }
                        if ((client != null))
                        {
                            client = null;
                        }
                    }
                    return readbytes;
                }
                catch (IOException ex)
                {

                    gloAuditTrail.gloAuditTrail.ExceptionLog(gloAuditTrail.ActivityModule.Prescription, gloAuditTrail.ActivityCategory.CreatePrescription, gloAuditTrail.ActivityType.General, ex.ToString(), gloAuditTrail.ActivityOutCome.Failure);
                    readbytes = null;
                    return null;
                }
                catch (EndpointNotFoundException ex)
                {
                    MessageBox.Show("Unable to Connect to server " + ex.ToString());
                    readbytes = null;
                    return null;
                }
                catch (TimeoutException ex)
                {
                    MessageBox.Show("Timeout expired. The timeout period elapsed prior to completion of the operation or the server is not responding.","gloEMR",MessageBoxButtons.OK,MessageBoxIcon.Error);
                    gloAuditTrail.gloAuditTrail.ExceptionLog(gloAuditTrail.ActivityModule.Prescription, gloAuditTrail.ActivityCategory.CreatePrescription, gloAuditTrail.ActivityType.General, ex.ToString(), gloAuditTrail.ActivityOutCome.Failure);
                    readbytes = null;
                    return null;
                }   
                catch (Exception ex)
                {
                    if (ex.Message.Contains("Timeout"))
                    {
                        //handle timeout
                        MessageBox.Show("Timeout expired. The timeout period elapsed prior to completion of the operation or the server is not responding.", "gloEMR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        MessageBox.Show("Unable to Connect to server " + ex.InnerException.ToString());
                    }
                    gloAuditTrail.gloAuditTrail.ExceptionLog(gloAuditTrail.ActivityModule.Prescription, gloAuditTrail.ActivityCategory.CreatePrescription, gloAuditTrail.ActivityType.General, ex.ToString(), gloAuditTrail.ActivityOutCome.Failure);
                    readbytes = null;
                    return null;
                }
                finally
                {
                    if (ds != null)
                    {
                        ds.Dispose();
                        ds = null;
                    }
                    oFile.Close();
                    oReader.Close();
                    //SLR: Dispose oFIle, oReader
                    oFile.Dispose();
                    oFile = null;
                    oReader.Dispose();
                    oReader = null;
                    readbytes = null;
                }
                
            }
            else
            {
                return null;
            }
        }

        public Cls271Information  Read271Response_5010(bool bShowMessage = false)
        {

            Cls271Information oCls271Information = new Cls271Information();
            ClsRxH_271Master oClsRxH_271Master = new ClsRxH_271Master();
            ClsRxH_271Details oClsRxH_271Details = new ClsRxH_271Details();
            oclsgloRxHubDBLayer = new clsgloRxHubDBLayer();
            string sAssemblyName = "";
            ediDocument dummy = null;
            ediSchemas dummySchemas = null;
            ediDocument oEdiDoc = myEDIObject(1, ref dummy);
            ediSchemas oSchemas = myEDISchemaObject(1, ref dummySchemas);
            //ediInterchange oInterchange = null;
            //ediGroup oGroup = null;
            //ediTransactionSet oTransactionset = null;
            ediDataSegment oSegment = null;

            ArrayList oSegmentArray = new ArrayList();
          //  ediDataSegment oNexSegment = null;
            //ediSchema oSchema = null;
            //ediSchemas oSchemas = null;
            //ediAcknowledgment oAck = null;
            try
            {
                /////since the same func is called from RxSniffer, therefore if this func is called from gloEMR then show error message boxes else dont show them.
                sAssemblyName = System.Reflection.Assembly.GetCallingAssembly().FullName;


                Boolean blnNM1Found = false;//if it is 2B then only we will save the EB data
                Boolean blnContractedProvider = false;
                Boolean blnPrimaryPayer = false;


                string sSegmentID = "";
                string sLoopSection = "";
                string sEntity = "";
                string Qlfr = "";
                string Qlfr_PharmacyCode = "";//Will be used for Retail-88 / MailOrder-99 / LTC - Long Term Care / Speciality 
                string Qlfr_MSGText = "";//Will be used for MSG segment to read Message Text  for Specialty Pharmacy or LTC 
                string Qlfr_InsuranceTypeCode = "";//if present then either  it will be 47/CP/MC/MP/OT
                string sRecieverID = "";
                string sSenderID = "";


                //Added for ANSI 5010
                //* This free text is used for Specialty Pharmacy and LTC since there is not a service type code available to use. 
                //The text SPECIALTY PHARMACY will indicate this EB loop is for Specialty Phamiacy and the text LTC will indicate this is for Long Term Care.
                //if this is blank then it means there is a loop for SPECIALTY PHARMACY OR LTC PHARMACY
                //Therefore depending on this we can take the eligibility/service dates and also use the MSG text decription from the MSG loop
                string sSpecialtyorLTCPhrmCovName = "";
                string SpecialtyorLTCPhrmEligibilityDate = "";
                string SpecialtyorLTCPhrmServiceDate = "";
                string sSpecialtyorLTCCoverageStatus = "";
                string sdtEligiblityDate = "12:00:00 AM";

             //   bool IsRetailPharmacy = false;
             //   bool IsMailOrdRx = false;


                int nArea;
             //   int myCount = 0;
                
                //StringBuilder sValue = new StringBuilder();
                 
                // Gets the first data segment in the EDI files
                
                ediDataSegment.Set(ref oSegment, (ediDataSegment)oEdiDoc.FirstDataSegment);  //oSegment = (ediDataSegment) oEdiDoc.FirstDataSegment
                oSegmentArray.Add(oSegment);
                oClsRxH_271Master.PatientId = _PatientId;//gloRxHub.ClsgloRxHubGeneral.gnPatientId;

                // This loop iterates though the EDI file a segment at a time
                while (oSegment != null)
                {
                    // A segment is identified by its Area number, Loop section and segment id.
                    sSegmentID = oSegment.ID;
                    sLoopSection = oSegment.LoopSection;
                    nArea = oSegment.Area;

                    #region "Area = 0"
                    if (nArea == 0)
                    {
                        if (sLoopSection == "")
                        {
                            #region "ISA"

                            if (sSegmentID == "ISA")
                            {
                                // map data elements of ISA segment in here
                                #region "Commented for ANSI 5010"
                                //sValue.Append("Authorization Information Qualifier :" + oSegment.get_DataElementValue(1) + Environment.NewLine);    //Authorization Information Qualifier
                                //sValue.Append("Authorization Information :" + oSegment.get_DataElementValue(2) + Environment.NewLine);    //Authorization Information
                                //sValue.Append("Security Information Qualifier :" + oSegment.get_DataElementValue(3) + Environment.NewLine);    //Security Information Qualifier
                                //sValue.Append("Security Information :" + oSegment.get_DataElementValue(4) + Environment.NewLine);    //Security Information
                                //sValue.Append("Interchange ID Qualifier :" + oSegment.get_DataElementValue(5) + Environment.NewLine);    //Interchange ID Qualifier
                                //sValue.Append("Interchange Sender ID :" + oSegment.get_DataElementValue(6) + Environment.NewLine);    //Interchange Sender ID
                                //sValue.Append("Sender ID :" + sSenderID + Environment.NewLine);    //Interchange Sender ID
                                //sValue.Append("Interchange ID Qualifier :" + oSegment.get_DataElementValue(7) + Environment.NewLine);    //Interchange ID Qualifier
                                //sValue.Append("Interchange Receiver ID" + oSegment.get_DataElementValue(8) + Environment.NewLine);    //Interchange Receiver ID
                                //sValue.Append("Receiver ID" + sRecieverID + Environment.NewLine);
                                //sValue.Append("Interchange Date :" + oSegment.get_DataElementValue(9) + Environment.NewLine);    //Interchange Date
                                //sValue.Append("Interchange Time :" + oSegment.get_DataElementValue(10) + Environment.NewLine);   //Interchange Time
                                //sValue.Append("Interchange Control Standards Identifier :" + oSegment.get_DataElementValue(11) + Environment.NewLine);   //Interchange Control Standards Identifier
                                //sValue.Append("Interchange Control Version Number :" + oSegment.get_DataElementValue(12) + Environment.NewLine);   //Interchange Control Version Number
                                //sValue.Append("Interchange Control Number :" + oSegment.get_DataElementValue(13) + Environment.NewLine);   //Interchange Control Number
                                //sValue.Append("Acknowledgment Requested :" + oSegment.get_DataElementValue(14) + Environment.NewLine);   //Acknowledgment Requested
                                //sValue.Append("Usage Indicator :" + oSegment.get_DataElementValue(15) + Environment.NewLine);   //Usage Indicator
                                //sValue.Append("Component Element Separator :" + oSegment.get_DataElementValue(16) + Environment.NewLine);   //Component Element Separator
                                #endregion "Commented for ANSI 5010"

                                sSenderID = oSegment.get_DataElementValue(6).Trim();

                                sRecieverID = oSegment.get_DataElementValue(8).Trim();

                                //This will be saved as message id in database
                                oClsRxH_271Master.ISA_ControlNumber = oSegment.get_DataElementValue(13);


                            }
                            #endregion "ISA"

                            #region "TA1"

                            //TA1 segment
                            else if (sSegmentID == "TA1")
                            {
                                //check for the code in the 5th element
                                oClsRxH_271Master.MessageType = "TA1" + "-" + oSegment.get_DataElementValue(04);
                                if (oSegment.get_DataElementValue(04) == "A")
                                {
                                    if (sAssemblyName.Contains("gloEMR"))
                                    {
                                        if (bShowMessage)
                                        {
                                            MessageBox.Show("The Transmitted Interchange Control Structure Header and Trailer Have Been Received and Have No Errors.", "gloEMR", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        }
                                    }

                                }
                                else if (oSegment.get_DataElementValue(04) == "R")
                                {
                                    StringBuilder strTA1Msg = new StringBuilder();
                                    strTA1Msg.Append("The Transmitted Interchange Control Structure Header");
                                    strTA1Msg.Append(Environment.NewLine);
                                    strTA1Msg.Append("and Trailer Have Been Received and Are Accepted But");
                                    strTA1Msg.Append(Environment.NewLine);
                                    strTA1Msg.Append("Errors Are Noted. This Means the Sender Must Not Resend This Data.");

                                    if (sAssemblyName.Contains("gloEMR"))
                                    {
                                        if (bShowMessage)
                                        {
                                            MessageBox.Show(strTA1Msg.ToString(), "gloEMR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        }
                                    }
                                    strTA1Msg = null;
                                }
                                else if (oSegment.get_DataElementValue(04) == "E")
                                {
                                    if (sAssemblyName.Contains("gloEMR"))
                                    {
                                        if (bShowMessage)
                                        {
                                            MessageBox.Show("The Transmitted Interchange Control Structure Header and Trailer are Rejected Because of Errors.", "gloEMR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        }
                                    }

                                }
                                oclsgloRxHubDBLayer.Connect(ClsgloRxHubGeneral.ConnectionString);
                                oclsgloRxHubDBLayer.InsertEDIResponse271_Details("sp_InUpRxH_RxH_271Response_Details", oClsRxH_271Master);
                                //return null;
                                break;
                            }
                            #endregion "TA1"

                            #region "GS"

                            else if (sSegmentID == "GS")
                            {
                                // map data elements of GS segment in here
                                #region "Commented for ANSI 5010"
                                //sValue.Append("Functional Identifier Code :" + oSegment.get_DataElementValue(1) + Environment.NewLine);  //Functional Identifier Code
                                //sValue.Append("Application Sender's Code :" + oSegment.get_DataElementValue(2) + Environment.NewLine);  //Application Sender's Code
                                //sValue.Append("Application Receiver's Code :" + oSegment.get_DataElementValue(3) + Environment.NewLine);  //Application Receiver's Code
                                //sValue.Append("Date :" + oSegment.get_DataElementValue(4) + Environment.NewLine);  //Date
                                //sValue.Append("Time :" + oSegment.get_DataElementValue(5) + Environment.NewLine);  //Time
                                //sValue.Append("Eligiblity Date : " + sdtEligiblityDate + Environment.NewLine);
                                //sValue.Append("Group Control Number :" + oSegment.get_DataElementValue(6) + Environment.NewLine);  //Group Control Number
                                //sValue.Append("Responsible Agency Code :" + oSegment.get_DataElementValue(7) + Environment.NewLine);  //Responsible Agency Code
                                //sValue.Append("Version :" + oSegment.get_DataElementValue(8));  //Version / Release
                                #endregion "Commented for ANSI 5010"

                                sdtEligiblityDate = oSegment.get_DataElementValue(4).Trim() + " " + oSegment.get_DataElementValue(5).Trim();

                            }
                            #endregion "GS"
                        }
                    }

                    #endregion "Area = 0"

                    #region "Area = 1"

                    else if (nArea == 1)
                    {
                        if (sLoopSection == "")
                        {
                            #region "ST"

                            if (sSegmentID == "ST")
                            {
                                // map data element of ST segment in here
                                if (oSegment.get_DataElementValue(1) != "")//means we have either 271 / 997
                                {
                                    if (oSegment.get_DataElementValue(1) == "997")
                                    {

                                        oClsRxH_271Master.MessageType = oSegment.get_DataElementValue(1);//either 271 / 997
                                        if (ClsgloRxHubGeneral.gblnSend270UsingDEA == false)
                                        {
                                            ClsgloRxHubGeneral.gblnSend270UsingDEA = true;
                                            oclsgloRxHubDBLayer.Connect(ClsgloRxHubGeneral.ConnectionString);
                                            oclsgloRxHubDBLayer.InsertEDIResponse271_Details("gsp_InUpRxH_RxH_271Response_Details", oClsRxH_271Master);
                                            oclsgloRxHubDBLayer.DeleteRxH_Table("gsp_DeleteRxHTables", oClsRxH_271Master.PatientId);
                                            break;
                                        }
                                        else
                                        {
                                            if (sAssemblyName.Contains("gloEMR"))
                                            {
                                                //MessageBox.Show("The 270 eligibility request file contains Error, 997 file responded!", "gloEMR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                if (bShowMessage)
                                                {
                                                    MessageBox.Show("The 270 eligibility request file contains invalid patient information. 997 error code responded in 271 response file", "gloEMR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                }
                                            }
                                            oclsgloRxHubDBLayer.Connect(ClsgloRxHubGeneral.ConnectionString);
                                            oclsgloRxHubDBLayer.InsertEDIResponse271_Details("sp_InUpRxH_RxH_271Response_Details", oClsRxH_271Master);
                                            oclsgloRxHubDBLayer.DeleteRxH_Table("sp_DeleteRxHTables", oClsRxH_271Master.PatientId);
                                            break;
                                        }////if response is 997 then RESEND the 270 file with DEA number

                                    }
                                    else
                                    {
                                        ClsgloRxHubGeneral.gblnSend270UsingDEA = false;////if response is NOT 997 then resend the 270 file with DEA number
                                        oClsRxH_271Master.MessageType = oSegment.get_DataElementValue(1);//either 271 / 997
                                        oClsRxH_271Master.STLoopCount = oSegment.get_DataElementValue(2);// STLoopCnt + 1;// oSegment.get_DataElementValue(2); //0001
                                    }

                                }
                                else
                                {
                                    oClsRxH_271Master.MessageType = "TA1";//wil be TA1. or NAK
                                }

                                //sValue.Append(oSegment.get_DataElementValue(1) + Environment.NewLine);
                                //sValue.Append(oSegment.get_DataElementValue(2) + Environment.NewLine); //00021
                            }

                            #endregion "ST"

                            #region "BHT"

                            else if (sSegmentID == "BHT")
                            {
                                //sValue.Append(oSegment.get_DataElementValue(3) + Environment.NewLine);
                                //this wil be saved as transaction ID in Database
                                oClsRxH_271Master.MessageID = oSegment.get_DataElementValue(3);
                            }
                            else if (sSegmentID == "REF")
                            {

                            }
                            #endregion "BHT"
                        }

                    }//Area ==1

                    #endregion "Area = 1"

                    #region "Area = 2"

                    else if (nArea == 2)
                    {
                        if (sLoopSection == "HL" && sSegmentID == "HL")
                        {
                            sEntity = oSegment.get_DataElementValue(3);
                        }

                        #region "Information Source - 20"

                        //****************************** Information Source 
                        if (sEntity == "20")
                        {
                            if (sLoopSection == "HL")
                            {
                                if (sSegmentID == "HL")
                                {

                                }
                                else if (sSegmentID == "AAA")
                                {

                                }

                            }//end loop section HL

                            #region "Information source 21 - HL;NM1"

                            else if (sLoopSection == "HL;NM1")
                            {
                                #region "HL;NM1 -> NM1"
                                if (sSegmentID == "NM1")
                                {
                                    oClsRxH_271Master.InformationSourceName = oSegment.get_DataElementValue(3) + " " + oSegment.get_DataElementValue(4) + " " + oSegment.get_DataElementValue(5);//PBM or payer name
                                    oClsRxH_271Master.PayerName = oSegment.get_DataElementValue(3);//PBM or payer name
                                    oClsRxH_271Master.PayerParticipantId = oSegment.get_DataElementValue(9);//PBM or payer participant ID returned in the NM109 will be used to populate drug history request transactions.
                                }
                                #endregion "HL;NM1 -> NM1"

                                #region "HL;NM1 -> REF"
                                else if (sSegmentID == "REF")
                                {

                                }
                                #endregion "HL;NM1 -> REF"

                                #region "HL;NM1 -> PER"
                                else if (sSegmentID == "PER")
                                {


                                }
                                #endregion "HL;NM1 -> PER"

                                #region "HL;NM1 -> AAA"

                                else if (sSegmentID == "AAA")
                                {
                                    if (oSegment.get_DataElementValue(1) == "N")
                                    {
                                        //sValue.Append(oSegment.get_DataElementValue(2) + Environment.NewLine);
                                        if (oSegment.get_DataElementValue(3).Trim() != "")
                                        {
                                            //listResponse.Items.Add("Payer Rejection Reason: " + GetSourceRejectionReason(oSegment.get_DataElementValue(3)));
                                            //listResponse.Items.Add("Payer Follow up: " + GetSourceFollowUp(oSegment.get_DataElementValue(4)));
                                        }

                                        //EDIReturnResult = oSegment.get_DataElementValue(3).Trim() + "-" + oSegment.get_DataElementValue(4).Trim();
                                        //AddNote(_PatientId, EDIReturnResult);//
                                    }
                                }
                                #endregion "HL;NM1 -> REF"

                            }//end loop section HL;NM1
                            #endregion "Information source 21 - HL;NM1"

                        }

                        #endregion "Information Source - 20"

                        #region "Information Reciever - 21"

                        //**************************** Information Reciever - physician
                        else if (sEntity == "21")
                        {
                            if (sLoopSection == "HL")
                            {

                                if (sSegmentID == "HL")
                                {
                                    //sValue.Append(oSegment.get_DataElementValue(1) + Environment.NewLine);
                                    //sValue.Append(oSegment.get_DataElementValue(2) + Environment.NewLine);
                                    //sValue.Append(oSegment.get_DataElementValue(3) + Environment.NewLine);
                                }
                            }

                            else if (sLoopSection == "HL;NM1")
                            {
                                #region "HL;NM1 -> NM1"
                                if (sSegmentID == "NM1")
                                {

                                    string RecieverLastName = oSegment.get_DataElementValue(3);
                                    string RecieverFirstName = oSegment.get_DataElementValue(4);
                                    string RecieverMiddleName = oSegment.get_DataElementValue(5);
                                    oClsRxH_271Master.InformationRecieverName = RecieverLastName + " " + RecieverFirstName + " " + RecieverMiddleName;//Information Reciever Name
                                    oClsRxH_271Master.InformationRecieverSuffix = oSegment.get_DataElementValue(7);

                                    string sCodeQlfr = "";
                                    sCodeQlfr = oSegment.get_DataElementValue(8);
                                    if (sCodeQlfr == "XX")
                                    {
                                        //Health Care Financing Administration National Provider Identifier
                                        //***The NPI is now mandated Surescripts will only reject if the NM1O8 and the NM1O9 are not populated.
                                        //Surescripts will not be validating the NPI. but some payers may validate it.
                                        oClsRxH_271Master.NPINumber = oSegment.get_DataElementValue(9);
                                    }

                                    #region "commented for ANSI 5010"

                                    #endregion "commented for ANSI 5010"

                                }
                                #endregion "HL;NM1 -> NM1"

                                #region "HL;NM1 -> REF"

                                else if (sSegmentID == "REF")
                                {

                                    //sValue.Append(oSegment.get_DataElementValue(1) + Environment.NewLine);
                                    //sValue.Append(oSegment.get_DataElementValue(2) + Environment.NewLine);
                                    //sValue.Append(oSegment.get_DataElementValue(3) + Environment.NewLine);

                                }
                                #endregion "HL;NM1 -> REF"

                                #region "HL;NM1 -> AAA"

                                else if (sSegmentID == "AAA")
                                {
                                    if (oSegment.get_DataElementValue(1) == "Y")
                                    {
                                        string sAAAQualifier = oSegment.get_DataElementValue(3);
                                        string sRejectionDescription = getRejecttionDescription(sAAAQualifier, "Reciever", "NM1");

                                        if (oClsRxH_271Master.MessageType != "")
                                        {
                                            //we will append a flag against the message type only if the condition is of type "Patient Not Found" 
                                            //and so in this way this will help the user to resend the 270 eligiblity request again for this patient
                                            oClsRxH_271Master.MessageType = oClsRxH_271Master.MessageType + "|" + sAAAQualifier;
                                        }
                                        if (sAssemblyName.Contains("gloEMR"))
                                        {
                                            if (bShowMessage)
                                            {
                                                MessageBox.Show("Transaction was unable to be processed successfully because " + sRejectionDescription, "gloEMR", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                            }
                                        }

                                        #region "Commented from ANSI 5010"

                                        ////check the 3rd value. if it is 67 then the patient is not present on RxHub database
                                        //if (oSegment.get_DataElementValue(3) == "67")
                                        //{
                                        //    if (oClsRxH_271Master.MessageType != "")
                                        //    {
                                        //        //we will append a flag against the message type only if the condition is of type "Patient Not Found" 
                                        //        //and so in this way this will help the user to resend the 270 eligiblity request again for this patient
                                        //        oClsRxH_271Master.MessageType = oClsRxH_271Master.MessageType + "|PNF";
                                        //    }
                                        //    if (sAssemblyName.Contains("gloEMR"))
                                        //    {
                                        //        MessageBox.Show("The request is valid, however the transaction has been rejected. Patient Not Found", "gloEMR", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                        //    }

                                        //    //break;
                                        //}
                                        //else if (oSegment.get_DataElementValue(3) == "41")//NCP = No contract with payer
                                        //{
                                        //    if (oClsRxH_271Master.MessageType != "")
                                        //    {
                                        //        //we will append a flag against the message type only if the condition is of type "" 
                                        //        //and so in this way this will help the user to resend the 270 eligiblity request again for this patient
                                        //        oClsRxH_271Master.MessageType = oClsRxH_271Master.MessageType + "|NCP";
                                        //    }
                                        //    if (sAssemblyName.Contains("gloEMR"))
                                        //    {
                                        //        MessageBox.Show("The request is valid, however the transaction has been rejected. No contract with payer", "gloEMR", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                        //    }

                                        //}
                                        //else if (oSegment.get_DataElementValue(3) == "42")//GSE=General system error
                                        //{
                                        //    if (oClsRxH_271Master.MessageType != "")
                                        //    {
                                        //        //we will append a flag against the message type only if the condition is of type "" 
                                        //        //and so in this way this will help the user to resend the 270 eligiblity request again for this patient
                                        //        oClsRxH_271Master.MessageType = oClsRxH_271Master.MessageType + "|GSE";
                                        //    }
                                        //    if (sAssemblyName.Contains("gloEMR"))
                                        //    {
                                        //        MessageBox.Show("The request is valid, however the transaction has been rejected. General system error", "gloEMR", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                        //    }

                                        //}
                                        #endregion "Commented from ANSI 5010"

                                    }
                                    else if (oSegment.get_DataElementValue(1) == "N")
                                    {
                                        //sValue.Append(oSegment.get_DataElementValue(2) + Environment.NewLine);
                                        if (oSegment.get_DataElementValue(3).Trim() != "")
                                        {
                                            //listResponse.Items.Add("Receiver Rejection Reason: " + GetReceiverRejectionReason(oSegment.get_DataElementValue(3)));
                                            //listResponse.Items.Add("Receiver Follow up: " + GetReceiverFollowUp(oSegment.get_DataElementValue(4)));
                                        }

                                        //EDIReturnResult = oSegment.get_DataElementValue(3).Trim() + "-" + oSegment.get_DataElementValue(4).Trim();

                                    }
                                }
                                #endregion "HL;NM1 -> AAA"
                            }

                        }

                        #endregion "Information Reciever - 21"

                        #region "Subscriber Level"

                        //*****************************Subscriber loop
                        else if (sEntity == "22")
                        {

                            if (sLoopSection == "HL")
                            {
                                if (sSegmentID == "HL")
                                {
                                    //sValue.Append(oSegment.get_DataElementValue(1) + Environment.NewLine);
                                    //sValue.Append(oSegment.get_DataElementValue(2) + Environment.NewLine);
                                    //sValue.Append(oSegment.get_DataElementValue(3) + Environment.NewLine);
                                }
                                else if (sSegmentID == "TRN")
                                {
                                    Qlfr = oSegment.get_DataElementValue(1);
                                    switch (Qlfr)
                                    {
                                        case "1":
                                            //string sTRNReferenceIdentification = oSegment.get_DataElementValue(2);
                                            oClsRxH_271Master.TRNReferenceIdentification = oSegment.get_DataElementValue(2);
                                            //string sTRNOrignationCompanyIdentifier = oSegment.get_DataElementValue(3);
                                            oClsRxH_271Master.TRNOrignationCompanyIdentifier = oSegment.get_DataElementValue(3);
                                            //string sTRNDivisionorGroup = oSegment.get_DataElementValue(4);
                                            oClsRxH_271Master.TRNDivisionorGroup = oSegment.get_DataElementValue(4);
                                            break;
                                        case "2":

                                            break;
                                    }
                                    //sValue.Append(oSegment.get_DataElementValue(1) + Environment.NewLine);
                                    //sValue.Append(oSegment.get_DataElementValue(2) + Environment.NewLine);
                                    //sValue.Append(oSegment.get_DataElementValue(3) + Environment.NewLine);
                                }
                            }

                            else if (sLoopSection == "HL;NM1")
                            {
                                #region "HL;NM1 -> NM1"
                                if (sSegmentID == "NM1")
                                {

                                    //sValue.Append("Subscriber Last Name : " + oSegment.get_DataElementValue(3) + Environment.NewLine);
                                    //sValue.Append("Subscriber First Name : " + oSegment.get_DataElementValue(4) + Environment.NewLine);
                                    //sValue.Append("Subscriber ID: " + oSegment.get_DataElementValue(9) + Environment.NewLine);

                                    oClsRxH_271Details.SubscriberLastName = oSegment.get_DataElementValue(3);//Subscriber Last Name

                                    oClsRxH_271Details.SubscriberFirstName = oSegment.get_DataElementValue(4);//Subscriber First Name

                                    oClsRxH_271Details.SubscriberMiddleName = oSegment.get_DataElementValue(5);//Subscriber Middle Name

                                    //Subscriber Suffix e.g., Sr., Jr., or III.
                                    oClsRxH_271Details.SubscriberSuffix = oSegment.get_DataElementValue(7);//Subscriber Suffix


                                    oClsRxH_271Master.MemberID = oSegment.get_DataElementValue(9);//Patient PBM/payer unique member ID will be used to populate drug history request and new prescription transactions.
                                }
                                #endregion "HL;NM1 -> NM1"

                                #region "HL;NM1 -> N3"
                                else if (sSegmentID == "N3")
                                {
                                    //sValue.Append("Subscriber Address : " + oSegment.get_DataElementValue(1) + Environment.NewLine);

                                    oClsRxH_271Details.SubscriberAddress1 = oSegment.get_DataElementValue(1);//Subscriber Address Line1

                                    oClsRxH_271Details.SubscriberAddress2 = oSegment.get_DataElementValue(2);//Subscriber Address Line2

                                }
                                #endregion "HL;NM1 -> N3"

                                #region "HL;NM1 -> N4"
                                else if (sSegmentID == "N4")
                                {
                                    //sValue.Append("Subscriber City : " + oSegment.get_DataElementValue(1) + Environment.NewLine);
                                    //sValue.Append("Subscriber State : " + oSegment.get_DataElementValue(2) + Environment.NewLine);
                                    //sValue.Append("Subscriber Zip : " + oSegment.get_DataElementValue(3) + Environment.NewLine);

                                    oClsRxH_271Details.SubscriberCity = oSegment.get_DataElementValue(1);//Subscriber City

                                    oClsRxH_271Details.SubscriberState = oSegment.get_DataElementValue(2);//Subscriber State

                                    oClsRxH_271Details.SubscriberZip = oSegment.get_DataElementValue(3);//Subscriber Zip

                                }
                                #endregion "HL;NM1 -> N4"

                                #region "HL;NM1 -> PER"
                                else if (sSegmentID == "PER")
                                {

                                }
                                #endregion "HL;NM1 -> PER"

                                #region "HL;NM1 -> REF"
                                else if (sSegmentID == "REF")
                                {
                                    Qlfr = oSegment.get_DataElementValue(1);

                                    switch (Qlfr)
                                    {
                                        case "A6":
                                            //sEmployeeId = oSegment.get_DataElementValue(2);
                                            //sValue.Append("Employee ID : " + sEmployeeId + Environment.NewLine);
                                            oClsRxH_271Master.EmployeeId = oSegment.get_DataElementValue(2);
                                            break;
                                        case "18":
                                            //sPlanNumber = oSegment.get_DataElementValue(2);
                                            //sValue.Append("Plan Number : " + sPlanNumber + Environment.NewLine);

                                            oClsRxH_271Master.HealthPlanNumber = oSegment.get_DataElementValue(2);
                                            oClsRxH_271Master.HealthPlanName = oSegment.get_DataElementValue(3);//health plan name in the REF03 
                                            break;
                                        case "1W":
                                            //sMemberID = oSegment.get_DataElementValue(2);
                                            //sValue.Append("Member ID : " + sMemberID + Environment.NewLine);

                                            oClsRxH_271Master.CardHolderId = oSegment.get_DataElementValue(2);//cardholder ID in the REF02 

                                            oClsRxH_271Master.CardHolderName = oSegment.get_DataElementValue(3);//and cardholder name in the REF03 will be used to populate drug history request and new prescription transactions.
                                            break;
                                        case "6P":
                                            //sGroupNumber = oSegment.get_DataElementValue(2);
                                            //sValue.Append("Group Number : " + sGroupNumber + Environment.NewLine);

                                            oClsRxH_271Master.GroupId = oSegment.get_DataElementValue(2);//group ID in the REF02 will be used to populate drug history request and new prescription transactions.
                                            oClsRxH_271Master.GroupName = oSegment.get_DataElementValue(3);//group ID in the REF02 will be used to populate drug history request and new prescription transactions.
                                            break;
                                        case "IF":
                                            //sFormularlyID = oSegment.get_DataElementValue(2);//formulary list ID in the REF02 
                                            //sValue.Append("Formulary ID : " + sFormularlyID + Environment.NewLine);

                                            oClsRxH_271Master.FormularyListId = oSegment.get_DataElementValue(2);//++++++++++++++++++++
                                            //sAlternativeID = oSegment.get_DataElementValue(3);
                                            //sValue.Append("Alternative ID : " + sAlternativeID + Environment.NewLine);
                                            oClsRxH_271Master.AlternativeListId = oSegment.get_DataElementValue(3);//++++++++++++++++++++
                                            break;
                                        case "1L":
                                            oClsRxH_271Master.CoverageId = oSegment.get_DataElementValue(3);//++++++++++++++++++++
                                            break;
                                        case "N6":
                                            //sBIN = oSegment.get_DataElementValue(2);
                                            //sValue.Append("BIN : " + sBIN + Environment.NewLine);
                                            oClsRxH_271Master.BINNumber = oSegment.get_DataElementValue(2);//++++++++++++++++++++
                                            break;
                                        case "IG":
                                            oClsRxH_271Master.CopayId = oSegment.get_DataElementValue(3);//++++++++++++++++++++
                                            break;
                                        case "HJ"://ANSI 5010
                                            //HJ        Identity Card Number (* Cardholder ID)
                                            //Strongly recommended by Surescripts.
                                            oClsRxH_271Master.CardHolderId = oSegment.get_DataElementValue(2);//cardholder ID in the REF02 
                                            oClsRxH_271Master.CardHolderName = oSegment.get_DataElementValue(3);
                                            break;
                                        case "49"://ANSI 5010
                                            //49        Family Unit Member (Person Code)
                                            oClsRxH_271Master.PersonCode = oSegment.get_DataElementValue(2);
                                            break;
                                        case "SY"://ANSI 5010
                                            //SY            Social Security Number
                                            //The social security number may not be used for any Federally administered programs such as Medicare.
                                            oClsRxH_271Master.SocialSecurityNumber = oSegment.get_DataElementValue(2);

                                            break;
                                        case "EJ"://ANSI 5010
                                            oClsRxH_271Master.PatientAccountNumber = oSegment.get_DataElementValue(2);
                                            break;

                                    }

                                    #region "Commented from ANSI 5010 - change if else... to switch case..."

                                    ////returned Employee ID
                                    //if (Qlfr == "A6")
                                    //{
                                    //    sEmployeeId = oSegment.get_DataElementValue(2);
                                    //    sValue.Append("Employee ID : " + sEmployeeId + Environment.NewLine);
                                    //    oClsRxH_271Master.EmployeeId = oSegment.get_DataElementValue(2);
                                    // }
                                    ////If returned, health plan name in the REF03 must be displayed in the end user application per RxHub application guideline 3.6.
                                    //if (Qlfr == "18")
                                    //{
                                    //    sPlanNumber = oSegment.get_DataElementValue(2);
                                    //    sValue.Append("Plan Number : " + sPlanNumber + Environment.NewLine);
                                    //    //txtHealthPlanName.Text = oSegment.get_DataElementValue(3);//++++++++++++++++++++
                                    //    oClsRxH_271Master.HealthPlanNumber = oSegment.get_DataElementValue(2);
                                    //    oClsRxH_271Master.HealthPlanName = oSegment.get_DataElementValue(3);//health plan name in the REF03 
                                    //}
                                    ////If returned, cardholder ID in the REF02 and cardholder name in the REF03 will be used to populate drug history request and new prescription transactions.
                                    //else if (Qlfr == "1W")
                                    //{
                                    //    sMemberID = oSegment.get_DataElementValue(2);
                                    //    sValue.Append("Member ID : " + sMemberID + Environment.NewLine);
                                    //    //txtCardHolderId.Text = oSegment.get_DataElementValue(2);//++++++++++++++++++++
                                    //    oClsRxH_271Master.CardHolderId = oSegment.get_DataElementValue(2);//cardholder ID in the REF02 
                                    //    //txtCardHolderName.Text = oSegment.get_DataElementValue(2);//++++++++++++++++++++
                                    //    oClsRxH_271Master.CardHolderName = oSegment.get_DataElementValue(3);//and cardholder name in the REF03 will be used to populate drug history request and new prescription transactions.
                                    //}
                                    ////If returned, group ID in the REF02 will be used to populate drug history request and new prescription transactions.
                                    //else if (Qlfr == "6P")
                                    //{
                                    //    sGroupNumber = oSegment.get_DataElementValue(2);
                                    //    sValue.Append("Group Number : " + sGroupNumber + Environment.NewLine);
                                    //    //txtGroupId.Text = oSegment.get_DataElementValue(2);//++++++++++++++++++++
                                    //    oClsRxH_271Master.GroupId = oSegment.get_DataElementValue(2);//group ID in the REF02 will be used to populate drug history request and new prescription transactions.
                                    //    oClsRxH_271Master.GroupName = oSegment.get_DataElementValue(3);//group ID in the REF02 will be used to populate drug history request and new prescription transactions.
                                    //}
                                    ////If returned, formulary list ID in the REF02 and alternative list ID in the REF03 will be used to link to patient formulary and benefit data.
                                    //else if (Qlfr == "IF")
                                    //{
                                    //    sFormularlyID = oSegment.get_DataElementValue(2);//formulary list ID in the REF02 
                                    //    sValue.Append("Formulary ID : " + sFormularlyID + Environment.NewLine);
                                    //    //txtFormularyListId.Text = oSegment.get_DataElementValue(2);//++++++++++++++++++++
                                    //    oClsRxH_271Master.FormularyListId = oSegment.get_DataElementValue(2);//++++++++++++++++++++
                                    //    sAlternativeID = oSegment.get_DataElementValue(3);
                                    //    sValue.Append("Alternative ID : " + sAlternativeID + Environment.NewLine);
                                    //    //txtAlternativeListId.Text = oSegment.get_DataElementValue(3);//++++++++++++++++++++
                                    //    oClsRxH_271Master.AlternativeListId = oSegment.get_DataElementValue(3);//++++++++++++++++++++
                                    //}
                                    ////If returned, coverage ID in the REF03 will be used to link to patient formulary and benefit data.
                                    //else if (Qlfr == "1L")
                                    //{
                                    //    //sFormularlyID = oSegment.get_DataElementValue(2);
                                    //    //sValue.Append("Formulary ID : " + sFormularlyID + Environment.NewLine);
                                    //    //txtCoverageId.Text = oSegment.get_DataElementValue(3);//++++++++++++++++++++
                                    //    oClsRxH_271Master.CoverageId = oSegment.get_DataElementValue(3);//++++++++++++++++++++
                                    //}
                                    ////If returned, BIN number in the REF02 will be used to populate new prescription transactions.
                                    //else if (Qlfr == "N6")
                                    //{
                                    //    sBIN = oSegment.get_DataElementValue(2);
                                    //    sValue.Append("BIN : " + sBIN + Environment.NewLine);
                                    //    //txtBinNumber.Text = oSegment.get_DataElementValue(2);//++++++++++++++++++++
                                    //    oClsRxH_271Master.BINNumber = oSegment.get_DataElementValue(2);//++++++++++++++++++++
                                    //    //sPCN = oSegment.get_DataElementValue(3);
                                    //    //sValue.Append("PCN : " + sPCN + Environment.NewLine);
                                    //}
                                    ////If returned, copay ID in the REF03 will be used to link to patient formulary and benefit data.
                                    //else if (Qlfr == "IG")
                                    //{
                                    //    //sBIN = oSegment.get_DataElementValue(2);
                                    //    //sValue.Append("BIN : " + sBIN + Environment.NewLine);
                                    //    //txtCopayId.Text = oSegment.get_DataElementValue(3);//++++++++++++++++++++
                                    //    oClsRxH_271Master.CopayId = oSegment.get_DataElementValue(3);//++++++++++++++++++++
                                    //    //sPCN = oSegment.get_DataElementValue(3);
                                    //    //sValue.Append("PCN : " + sPCN + Environment.NewLine);
                                    //}

                                    #endregion "Commented from ANSI 5010"



                                }
                                #endregion "HL;NM1 -> REF"


                                #region "AAA - Subscriber Request Validation"

                                else if (sSegmentID == "AAA")
                                {
                                    if (oSegment.get_DataElementValue(1) == "Y")
                                    {
                                        string sAAAQualifier = oSegment.get_DataElementValue(3);
                                        string sRejectionDescription = getRejecttionDescription(sAAAQualifier, "Subscriber", "NM1");

                                        if (oClsRxH_271Master.MessageType != "")
                                        {
                                            //we will append a flag against the message type only if the condition is of type "Patient Not Found" 
                                            //and so in this way this will help the user to resend the 270 eligiblity request again for this patient
                                            oClsRxH_271Master.MessageType = oClsRxH_271Master.MessageType + "|" + sAAAQualifier;
                                        }
                                        if (sAssemblyName.Contains("gloEMR"))
                                        {
                                            if (bShowMessage)
                                            {
                                                MessageBox.Show("Transaction was unable to be processed successfully for PBM: " + oClsRxH_271Master.PayerName + " because " + sRejectionDescription, "gloEMR", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                            }
                                        }

                                        #region "Commented from ANSI 5010"

                                        ////check the 3rd value. if it is 67 then the patient is not present on RxHub database
                                        //if (oSegment.get_DataElementValue(3) == "67")
                                        //{
                                        //    if (oClsRxH_271Master.MessageType != "")
                                        //    {
                                        //        //we will append a flag against the message type only if the condition is of type "Patient Not Found" 
                                        //        //and so in this way this will help the user to resend the 270 eligiblity request again for this patient
                                        //        oClsRxH_271Master.MessageType = oClsRxH_271Master.MessageType + "|PNF";
                                        //    }
                                        //    if (sAssemblyName.Contains("gloEMR"))
                                        //    {
                                        //        MessageBox.Show("The request is valid, however the transaction has been rejected. Patient Not Found", "gloEMR", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                        //    }

                                        //    //break;
                                        //}
                                        //else if (oSegment.get_DataElementValue(3) == "41")//NCP = No contract with payer
                                        //{
                                        //    if (oClsRxH_271Master.MessageType != "")
                                        //    {
                                        //        //we will append a flag against the message type only if the condition is of type "" 
                                        //        //and so in this way this will help the user to resend the 270 eligiblity request again for this patient
                                        //        oClsRxH_271Master.MessageType = oClsRxH_271Master.MessageType + "|NCP";
                                        //    }
                                        //    if (sAssemblyName.Contains("gloEMR"))
                                        //    {
                                        //        MessageBox.Show("The request is valid, however the transaction has been rejected. No contract with payer", "gloEMR", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                        //    }

                                        //}
                                        //else if (oSegment.get_DataElementValue(3) == "42")//GSE=General system error
                                        //{
                                        //    if (oClsRxH_271Master.MessageType != "")
                                        //    {
                                        //        //we will append a flag against the message type only if the condition is of type "" 
                                        //        //and so in this way this will help the user to resend the 270 eligiblity request again for this patient
                                        //        oClsRxH_271Master.MessageType = oClsRxH_271Master.MessageType + "|GSE";
                                        //    }
                                        //    if (sAssemblyName.Contains("gloEMR"))
                                        //    {
                                        //        MessageBox.Show("The request is valid, however the transaction has been rejected. General system error", "gloEMR", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                        //    }

                                        //}
                                        #endregion "Commented from ANSI 5010"
                                    }
                                    else
                                    {
                                        if (sAssemblyName.Contains("gloEMR"))
                                        {
                                            if (bShowMessage)
                                            {
                                                MessageBox.Show("The request or an element in the request is not valid. The transaction has been rejected", "gloEMR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                            }
                                        }


                                        break;
                                        //sValue.Append(oSegment.get_DataElementValue(2) + Environment.NewLine);
                                        //if (oSegment.get_DataElementValue(3).Trim() != "")
                                        //{
                                        //    //listResponse.Items.Add("Payer Rejection Reason: " + GetSubscriberRejectionReason(oSegment.get_DataElementValue(3)));
                                        //    //listResponse.Items.Add("Payer Follow up: " + GetSubscriberFollowUp(oSegment.get_DataElementValue(4)));
                                        //}

                                        //EDIReturnResult = oSegment.get_DataElementValue(3).Trim() + "-" + oSegment.get_DataElementValue(4).Trim();

                                    }
                                }

                                #endregion "AAA - Subscriber Request Validation"

                                #region "DMG - Subscriber Demographic Information"

                                else if (sSegmentID == "DMG")
                                {
                                    //sValue.Append("Subscriber Demographic Information : " + oSegment.get_DataElementValue(1) + Environment.NewLine);
                                    //sValue.Append("Subscriber Date of Birth : " + oSegment.get_DataElementValue(2) + Environment.NewLine);
                                    //sValue.Append("Subscriber Gender : " + oSegment.get_DataElementValue(3) + Environment.NewLine);
                                    oClsRxH_271Details.SubscriberDOB = oSegment.get_DataElementValue(2);//Subscriber Date of birth

                                    oClsRxH_271Details.SubscriberGender = oSegment.get_DataElementValue(3);//Subscriber Gender

                                }

                                #endregion "DMG - Subscriber Demographic Information"

                                #region "INS - Subscriber Relationship"

                                else if (sSegmentID == "INS")
                                {
                                    //If INS03 is populated with �001� and INS04 is populated with �25�, some patient demographic information returned in the 271 response differs from what was submitted in the 270 request
                                    if (oSegment.get_DataElementValue(3) == "001" && oSegment.get_DataElementValue(4) == "25")
                                    {

                                        oClsRxH_271Details.IsSubscriberdemoChange = "True";
                                        //%%%%%%%%%%%%%%%%%%%%%%%%PATIENT MATCH VERIFICATION

                                        //*********since in oPatient.DOB we have binded the date in {7/5/1975 12:00:00 AM}	format so in 271 file generation we send 19750705(year month day) format, so to match we hv written the following code.
                                        //string _PatientDOB = "";
                                        //if (oPatient.DOB != null)
                                        //{
                                        //    _PatientDOB = oPatient.DOB.Date.ToString("yyyyMMdd");
                                        //}
                                        //*********

                                        //*********since in oPatient.Gender we have binded "Male", "Female", "Other" so in 271 file generation we send "M","F","O" so to match we hv written the following code.
                                        //string _PatientGender = "";

                                        //if (oPatient.Gender == "Male")
                                        //{
                                        //    _PatientGender = "M";
                                        //}
                                        //else if (oPatient.Gender == "Female")
                                        //{
                                        //    _PatientGender = "F";
                                        //}
                                        //else if (oPatient.Gender == "Other")
                                        //{
                                        //    _PatientGender = "O";
                                        //}
                                        //else if (oPatient.Gender == "Unknown")
                                        //{
                                        //    _PatientGender = "U";
                                        //}
                                        //*********
                                        //StringBuilder strmessage = new StringBuilder();
                                        //strmessage.Append("Patient Match Verification : The Response 271 file has different data than that of 270 Request file");
                                        //strmessage.Append(Environment.NewLine);

                                        //StringBuilder str270PatientDemographics = new StringBuilder();
                                        //str270PatientDemographics.Append("270 Request contains : " + oPatient.LastName + " " + oPatient.FirstName + " " + oPatient.MiddleName + " " + oPatient.PatientAddress.Zip + " " + _PatientDOB + " " + _PatientGender);
                                        //str270PatientDemographics.Append(Environment.NewLine);

                                        //strmessage.Append(str270PatientDemographics);

                                        //StringBuilder str271PatientDemographics = new StringBuilder();
                                        //str271PatientDemographics.Append("271 Response contains : " + oClsRxH_271Details.SubscriberLastName + " " + oClsRxH_271Details.SubscriberFirstName + " " + oClsRxH_271Details.SubscriberMiddleName + " " + oClsRxH_271Details.SubscriberZip + " " + oClsRxH_271Details.SubscriberDOB + " " + oClsRxH_271Details.SubscriberGender);
                                        oClsRxH_271Details.SubscriberDemoChgLastName = oClsRxH_271Details.SubscriberLastName;
                                        oClsRxH_271Details.SubscriberDemochgFirstName = oClsRxH_271Details.SubscriberFirstName;
                                        oClsRxH_271Details.SubscriberDemoChgMiddleName = oClsRxH_271Details.SubscriberMiddleName;
                                        oClsRxH_271Details.SubscriberDemoChgZip = oClsRxH_271Details.SubscriberZip;
                                        oClsRxH_271Details.SubscriberDemoChgDOB = oClsRxH_271Details.SubscriberDOB;
                                        oClsRxH_271Details.SubscriberDemoChgGender = oClsRxH_271Details.SubscriberGender;
                                        oClsRxH_271Details.SubscriberDemoChgState = oClsRxH_271Details.SubscriberState;
                                        oClsRxH_271Details.SubscriberDemoChgCity = oClsRxH_271Details.SubscriberCity;
                                        oClsRxH_271Details.SubscriberDemoChgAddress1 = oClsRxH_271Details.SubscriberAddress1;
                                        oClsRxH_271Details.SubscriberDemoChgAddress2 = oClsRxH_271Details.SubscriberAddress2;

                                        //strmessage.Append(str271PatientDemographics);

                                        //MessageBox.Show(strmessage.ToString(), "gloRxHub", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        //%%%%%%%%%%%%%%%%%%%%%%%%PATIENT MATCH VERIFICATION
                                    }
                                    else//patient demographich has not changed
                                    {

                                        oClsRxH_271Details.IsSubscriberdemoChange = "False";
                                    }

                                    if (oSegment.get_DataElementValue(1) == "Y")
                                    {
                                        if (oSegment.get_DataElementValue(2) == "18")
                                        {
                                            oClsRxH_271Master.RelationshipCode = oSegment.get_DataElementValue(2);
                                            oClsRxH_271Master.RelationshipDescription = "Self";
                                        }

                                    }
                                    else//subscriber is dependant
                                    {
                                        oClsRxH_271Master.RelationshipCode = oSegment.get_DataElementValue(2);
                                        oClsRxH_271Master.RelationshipDescription = "Dependent";
                                    }
                                }

                                #endregion "INS - Subscriber Relationship"

                                #region "DTP"

                                else if (sSegmentID == "DTP")
                                {
                                    Qlfr = oSegment.get_DataElementValue(1);
                                    switch (Qlfr)
                                    {
                                        case "307"://Eligiblity Range of dates when the subscriber or dependent were eligible for benefits
                                            oClsRxH_271Master.EligiblityDate = oSegment.get_DataElementValue(3);
                                            break;
                                        case "472"://Service (Recommended by RxHub) Begin and end dates of the service being rendered
                                            //Added for ANSI 5010
                                            Qlfr = oSegment.get_DataElementValue(2);
                                            if (Qlfr == "D8")//Eligiblity Range of dates when the subscriber or dependent were eligible for benefits
                                            {
                                                oClsRxH_271Master.EligiblityDate = oSegment.get_DataElementValue(3);
                                            }
                                            else if (Qlfr == "RD8")//Service (Recommended by RxHub) Begin and end dates of the service being rendered
                                            {
                                                oClsRxH_271Master.ServiceDate = oSegment.get_DataElementValue(3);
                                            }

                                            break;
                                        case "291":
                                            //Added for ANSI 5010
                                            Qlfr = oSegment.get_DataElementValue(2);
                                            if (Qlfr == "D8")//Eligiblity Range of dates when the subscriber or dependent were eligible for benefits
                                            {
                                                oClsRxH_271Master.EligiblityDate = oSegment.get_DataElementValue(3);
                                            }
                                            else if (Qlfr == "RD8")//Service (Recommended by RxHub) Begin and end dates of the service being rendered
                                            {
                                                oClsRxH_271Master.ServiceDate = oSegment.get_DataElementValue(3);
                                            }
                                            break;
                                    }


                                    //sValue.Append("Subscriber Service : " + oSegment.get_DataElementValue(1) + Environment.NewLine);
                                    //sValue.Append("Subscriber Date : " + oSegment.get_DataElementValue(2) + Environment.NewLine);
                                    //sValue.Append("Subscriber Service Date : " + oSegment.get_DataElementValue(3) + Environment.NewLine);

                                }
                                #endregion "DTP"
                            }

                            #region "EB - Subscriber Eligibility or Benefit Information"

                            else if (sLoopSection == "HL;NM1;EB")
                            {

                                if (sSegmentID == "EB")
                                {
                                    Qlfr = oSegment.get_DataElementValue(1);

                                    //* This free text is used for Specialty Pharmacy and LTC since there is not a service type
                                    //code available to use. The text SPECIALTY PHARMACY will indicate this EB loop is for
                                    //Specialty Phamiacy and the text LTC will indicate this is for Long Term Care.
                                    Qlfr_PharmacyCode = oSegment.get_DataElementValue(3); //Pharmacy for which eligibility coverage is considered
                                }
                                if (sSegmentID == "EB")
                                {
                                    if (blnNM1Found == false)
                                    {
                                        //Only if the Qlfr =1 tht means pharnacy has active coverage for claim then make the IsPharmacy flag to true,
                                        //else keep it false
                                        #region " Subscriber EB Segment (When NM1 is IL) "

                                        #region "1 - Active"

                                        if (Qlfr == "1")//Active Coverage
                                        {
                                            #region "88 - Retail Pharmacy Coverage"

                                            if (oSegment.get_DataElementValue(3) == "88")//Retail coverage
                                            {
                                                if (oSegment.SegmentBuffer.Contains("EB18890"))
                                                {
                                                    oClsRxH_271Master.IsMailOrdRxDrugEligible = "Yes";
                                                    oClsRxH_271Master.MailOrdRxDrugEligiblityorBenefitInfo = "Active Coverage";
                                                }
                                                oClsRxH_271Master.IsRetailPharmacyEligible = "Yes";

                                                #region "Insurance type code - Retail"
                                                Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
                                                if (Qlfr_InsuranceTypeCode != "")
                                                {
                                                    if (Qlfr_InsuranceTypeCode == "47")
                                                    {
                                                        oClsRxH_271Master.RetailInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                    }
                                                    else if (Qlfr_InsuranceTypeCode == "CP")
                                                    {
                                                        oClsRxH_271Master.RetailInsTypeCode = "Medicare Conditionally Primary";
                                                    }
                                                    else if (Qlfr_InsuranceTypeCode == "MC")
                                                    {
                                                        oClsRxH_271Master.RetailInsTypeCode = "Medicaid";
                                                    }
                                                    else if (Qlfr_InsuranceTypeCode == "MP")
                                                    {
                                                        oClsRxH_271Master.RetailInsTypeCode = "Medicare Primary";
                                                    }
                                                    else if (Qlfr_InsuranceTypeCode == "OT")
                                                    {
                                                        oClsRxH_271Master.RetailInsTypeCode = "Other (Used for Medicare Part D)";
                                                    }

                                                }
                                                #endregion "Insurance type code - Retail"

                                                //oClsRxH_271Master.RetailInsTypeCode = oSegment.get_DataElementValue(4);
                                                oClsRxH_271Master.RetailPharmacyCoveragePlanName = oSegment.get_DataElementValue(5);
                                                oClsRxH_271Master.RetailMonetaryAmount = oSegment.get_DataElementValue(7);
                                                oClsRxH_271Master.RetailPharmacyEligiblityorBenefitInfo = "Active Coverage";
                                          //      IsRetailPharmacy = true;

                                            }
                                            #endregion "88 - Retail Pharmacy Coverage"

                                            #region "90 - Mail Order Pharmacy Coverage"

                                            if (oSegment.get_DataElementValue(3) == "90")//Mail coverage
                                            {
                                                oClsRxH_271Master.IsMailOrdRxDrugEligible = "Yes";
                                                #region "Insurance type code - Mail"
                                                if (Qlfr_InsuranceTypeCode != "")
                                                {
                                                    switch (Qlfr_InsuranceTypeCode)
                                                    {
                                                        case "47":
                                                            oClsRxH_271Master.RetailInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                            break;
                                                        case "CP":
                                                            oClsRxH_271Master.RetailInsTypeCode = "Medicare Conditionally Primary";
                                                            break;
                                                        case "MC":
                                                            oClsRxH_271Master.RetailInsTypeCode = "Medicaid";
                                                            break;
                                                        case "MP":
                                                            oClsRxH_271Master.RetailInsTypeCode = "Medicare Primary";
                                                            break;
                                                        case "OT":
                                                            oClsRxH_271Master.RetailInsTypeCode = "Other (Used for Medicare Part D)";
                                                            break;


                                                    }

                                                }
                                                #endregion "Insurance type code - Mail"
                                                //oClsRxH_271Master.MailOrderInsTypeCode = oSegment.get_DataElementValue(4);
                                                oClsRxH_271Master.MailOrdRxDrugCoveragePlanName = oSegment.get_DataElementValue(5);
                                                oClsRxH_271Master.MailOrderMonetaryAmount = oSegment.get_DataElementValue(7);
                                                oClsRxH_271Master.MailOrdRxDrugEligiblityorBenefitInfo = "Active Coverage";
                                             //   IsMailOrdRx = true;

                                            }
                                            #endregion "90 - Mail Order Pharmacy Coverage"

                                            #region "30 - Health Plan Coverage"
                                            //Added for ANSI 5010 
                                            if (oSegment.get_DataElementValue(3) == "30")//Health Plan Benefit Coverage
                                            {
                                                #region "Insurance type code"

                                                Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);

                                                if (Qlfr_InsuranceTypeCode != "")
                                                {
                                                    switch (Qlfr_InsuranceTypeCode)
                                                    {
                                                        case "47":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                            break;
                                                        case "CP":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Medicare Conditionally Primary";
                                                            break;
                                                        case "MC":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Medicaid";
                                                            break;
                                                        case "MP":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Medicare Primary";
                                                            break;
                                                        case "OT":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Other (Used for Medicare Part D)";
                                                            break;

                                                    }
                                                }

                                                #endregion "Insurance type code"
                                                oClsRxH_271Master.HealthPlanBenefitEligibilityInfo = "Active Coverage";
                                                oClsRxH_271Master.HealthPlanBenefitCoverageName = oSegment.get_DataElementValue(5);//added for ANSI 5010
                                            }
                                            #endregion "30 - Health Plan Coverage"

                                            #region "Blank/Null/Empty - LTC/Specialty Pharmacy Coverage"

                                            //Added for ANSI 5010 
                                            // if Empty/Null        Specialty Phamacy or LTC (See MSG segment) pg 133 of "Prescription Benefit IG 2011-04-15.pdf"
                                            if (oSegment.get_DataElementValue(3) == "")
                                            {
                                                #region "Insurance type code"

                                                Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);

                                                if (Qlfr_InsuranceTypeCode != "")
                                                {
                                                    switch (Qlfr_InsuranceTypeCode)
                                                    {
                                                        case "47":
                                                            break;

                                                        case "CP":
                                                            break;

                                                        case "MC":
                                                            break;

                                                        case "MP":
                                                            break;

                                                        case "OT":
                                                            break;

                                                    }
                                                }

                                                #endregion "Insurance type code"
                                                //string sHealthPlanBenefitCoverageName = "";//new property proc need to be added instead of string variable
                                                sSpecialtyorLTCPhrmCovName = oSegment.get_DataElementValue(5);//new property proc need to be added instead of string variable
                                                sSpecialtyorLTCCoverageStatus = "Active Coverage";
                                            }
                                            #endregion "Blank/Null/Empty - LTC/Specialty Pharmacy Coverage"

                                        }
                                        #endregion "1 - Active"

                                        #region "6 - Inactive"

                                        else if (Qlfr == "6")//Inactive - if inactive then Retail-99 and Mail-88 loops will not be sent
                                        {
                                            //6         Inactive
                                            //*If the member is inactive, then no other EB loops are required to be sent.

                                            if (oSegment.get_DataElementValue(3) == "30")//Health Plan Benefit Coverage
                                            {
                                                #region "Insurance type code"

                                                Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);

                                                if (Qlfr_InsuranceTypeCode != "")
                                                {
                                                    switch (Qlfr_InsuranceTypeCode)
                                                    {
                                                        case "47":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                            break;
                                                        case "CP":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Medicare Conditionally Primary";
                                                            break;
                                                        case "MC":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Medicaid";
                                                            break;
                                                        case "MP":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Medicare Primary";
                                                            break;
                                                        case "OT":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Other (Used for Medicare Part D)";
                                                            break;

                                                    }
                                                }

                                                #endregion "Insurance type code"

                                                oClsRxH_271Master.HealthPlanBenefitEligibilityInfo = "Inactive";
                                                oClsRxH_271Master.HealthPlanBenefitCoverageName = oSegment.get_DataElementValue(5);//added for ANSI 5010
                                            }
                                            else if (oSegment.get_DataElementValue(3).Trim() == "88")
                                            {
                                                #region "Insurance type code"

                                                Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);

                                                if (Qlfr_InsuranceTypeCode != "")
                                                {
                                                    switch (Qlfr_InsuranceTypeCode)
                                                    {
                                                        case "47":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                            break;
                                                        case "CP":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Medicare Conditionally Primary";
                                                            break;
                                                        case "MC":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Medicaid";
                                                            break;
                                                        case "MP":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Medicare Primary";
                                                            break;
                                                        case "OT":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Other (Used for Medicare Part D)";
                                                            break;

                                                    }
                                                }

                                                #endregion "Insurance type code"

                                                oClsRxH_271Master.IsRetailPharmacyEligible = "NO";
                                                oClsRxH_271Master.RetailMonetaryAmount = oSegment.get_DataElementValue(7);
                                                oClsRxH_271Master.RetailPharmacyEligiblityorBenefitInfo = "Inactive";
                                           //     IsRetailPharmacy = true;
                                            }
                                            else if (oSegment.get_DataElementValue(3).Trim() == "90")
                                            {
                                                #region "Insurance type code"

                                                Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);

                                                if (Qlfr_InsuranceTypeCode != "")
                                                {
                                                    switch (Qlfr_InsuranceTypeCode)
                                                    {
                                                        case "47":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                            break;
                                                        case "CP":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Medicare Conditionally Primary";
                                                            break;
                                                        case "MC":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Medicaid";
                                                            break;
                                                        case "MP":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Medicare Primary";
                                                            break;
                                                        case "OT":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Other (Used for Medicare Part D)";
                                                            break;

                                                    }
                                                }

                                                #endregion "Insurance type code"
                                                oClsRxH_271Master.IsMailOrdRxDrugEligible = "NO";
                                                oClsRxH_271Master.MailOrdRxDrugEligiblityorBenefitInfo = "Inactive";
                                                oClsRxH_271Master.MailOrderMonetaryAmount = oSegment.get_DataElementValue(7);
                                          //      IsMailOrdRx = true;
                                            }
                                            else if (oSegment.get_DataElementValue(3).Trim() == "")
                                            {
                                                #region "Insurance type code"

                                                Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);

                                                if (Qlfr_InsuranceTypeCode != "")
                                                {
                                                    switch (Qlfr_InsuranceTypeCode)
                                                    {
                                                        case "47":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                            break;
                                                        case "CP":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Medicare Conditionally Primary";
                                                            break;
                                                        case "MC":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Medicaid";
                                                            break;
                                                        case "MP":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Medicare Primary";
                                                            break;
                                                        case "OT":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Other (Used for Medicare Part D)";
                                                            break;

                                                    }
                                                }

                                                #endregion "Insurance type code"

                                                sSpecialtyorLTCCoverageStatus = "Inactive";


                                            }




                                            #region "commented for ANSI 5010"
                                            //commented for ANSI 5010
                                            //if (oSegment.get_DataElementValue(3).Trim() == "88")
                                            //{
                                            //    oClsRxH_271Master.IsRetailPharmacyEligible = "NO";
                                            //    #region "Insurance type code - Retail"
                                            //    Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
                                            //    if (Qlfr_InsuranceTypeCode != "")
                                            //    {
                                            //        if (Qlfr_InsuranceTypeCode == "47")
                                            //        {
                                            //            oClsRxH_271Master.RetailInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                            //        }
                                            //        else if (Qlfr_InsuranceTypeCode == "CP")
                                            //        {
                                            //            oClsRxH_271Master.RetailInsTypeCode = "Medicare Conditionally Primary";
                                            //        }
                                            //        else if (Qlfr_InsuranceTypeCode == "MC")
                                            //        {
                                            //            oClsRxH_271Master.RetailInsTypeCode = "Medicaid";
                                            //        }
                                            //        else if (Qlfr_InsuranceTypeCode == "MP")
                                            //        {
                                            //            oClsRxH_271Master.RetailInsTypeCode = "Medicare Primary";
                                            //        }
                                            //        else if (Qlfr_InsuranceTypeCode == "OT")
                                            //        {
                                            //            oClsRxH_271Master.RetailInsTypeCode = "Other (Used for Medicare Part D)";
                                            //        }

                                            //    }
                                            //    #endregion "Insurance type code - Retail"
                                            //    //oClsRxH_271Master.RetailInsTypeCode = oSegment.get_DataElementValue(4);
                                            //    //oClsRxH_271Master.PharmacyCoveragePlanName = oSegment.get_DataElementValue(5);
                                            //    oClsRxH_271Master.RetailMonetaryAmount = oSegment.get_DataElementValue(7);
                                            //    oClsRxH_271Master.RetailPharmacyEligiblityorBenefitInfo = "Inactive";
                                            //    IsRetailPharmacy = true;

                                            //}
                                            //if (oSegment.get_DataElementValue(3) == "90")
                                            //{
                                            //    oClsRxH_271Master.IsMailOrdRxDrugEligible = "NO";
                                            //    #region "Insurance type code - Mail"
                                            //    Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
                                            //    if (Qlfr_InsuranceTypeCode != "")
                                            //    {
                                            //        if (Qlfr_InsuranceTypeCode == "47")
                                            //        {
                                            //            oClsRxH_271Master.MailOrderInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                            //        }
                                            //        else if (Qlfr_InsuranceTypeCode == "CP")
                                            //        {
                                            //            oClsRxH_271Master.MailOrderInsTypeCode = "Medicare Conditionally Primary";
                                            //        }
                                            //        else if (Qlfr_InsuranceTypeCode == "MC")
                                            //        {
                                            //            oClsRxH_271Master.MailOrderInsTypeCode = "Medicaid";
                                            //        }
                                            //        else if (Qlfr_InsuranceTypeCode == "MP")
                                            //        {
                                            //            oClsRxH_271Master.MailOrderInsTypeCode = "Medicare Primary";
                                            //        }
                                            //        else if (Qlfr_InsuranceTypeCode == "OT")
                                            //        {
                                            //            oClsRxH_271Master.MailOrderInsTypeCode = "Other (Used for Medicare Part D)";
                                            //        }

                                            //    }
                                            //    #endregion "Insurance type code - Mail"
                                            //    //oClsRxH_271Master.MailOrderInsTypeCode = oSegment.get_DataElementValue(4);
                                            //    // oClsRxH_271Master.MailOrdRxDrugCoveragePlanName = oSegment.get_DataElementValue(5);
                                            //    oClsRxH_271Master.MailOrdRxDrugEligiblityorBenefitInfo = "Inactive";
                                            //    oClsRxH_271Master.MailOrderMonetaryAmount = oSegment.get_DataElementValue(7);
                                            //    IsMailOrdRx = true;

                                            //}
                                            //commented for ANSI 5010
                                            #endregion "commented for ANSI 5010"

                                        }
                                        #endregion "6 - Inactive"

                                        #region "G - Out Of Pocket (Stop Loss)"

                                        else if (Qlfr == "G")//Out Of Pocket (Stop Loss)
                                        {
                                            //listResponse.Items.Add("Deductibles: " + oSegment.get_DataElementValue(1).Trim());
                                            if (oSegment.get_DataElementValue(3).Trim() == "88")
                                            {
                                                oClsRxH_271Master.IsRetailPharmacyEligible = "NO";
                                                #region "Insurance type code - Retail"
                                                Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
                                                if (Qlfr_InsuranceTypeCode != "")
                                                {
                                                    if (Qlfr_InsuranceTypeCode == "47")
                                                    {
                                                        oClsRxH_271Master.RetailInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                    }
                                                    else if (Qlfr_InsuranceTypeCode == "CP")
                                                    {
                                                        oClsRxH_271Master.RetailInsTypeCode = "Medicare Conditionally Primary";
                                                    }
                                                    else if (Qlfr_InsuranceTypeCode == "MC")
                                                    {
                                                        oClsRxH_271Master.RetailInsTypeCode = "Medicaid";
                                                    }
                                                    else if (Qlfr_InsuranceTypeCode == "MP")
                                                    {
                                                        oClsRxH_271Master.RetailInsTypeCode = "Medicare Primary";
                                                    }
                                                    else if (Qlfr_InsuranceTypeCode == "OT")
                                                    {
                                                        oClsRxH_271Master.RetailInsTypeCode = "Other (Used for Medicare Part D)";
                                                    }

                                                }
                                                #endregion "Insurance type code - Retail"
                                                oClsRxH_271Master.RetailPharmacyCoveragePlanName = oSegment.get_DataElementValue(5);
                                                oClsRxH_271Master.RetailPharmacyEligiblityorBenefitInfo = "Out of Pocket (Stop Loss)";
                                                oClsRxH_271Master.RetailMonetaryAmount = oSegment.get_DataElementValue(7);
                                         //       IsRetailPharmacy = true;

                                            }
                                            if (oSegment.get_DataElementValue(3) == "90")
                                            {
                                                oClsRxH_271Master.IsMailOrdRxDrugEligible = "NO";
                                                #region "Insurance type code - Mail"
                                                Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
                                                if (Qlfr_InsuranceTypeCode != "")
                                                {
                                                    if (Qlfr_InsuranceTypeCode == "47")
                                                    {
                                                        oClsRxH_271Master.MailOrderInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                    }
                                                    else if (Qlfr_InsuranceTypeCode == "CP")
                                                    {
                                                        oClsRxH_271Master.MailOrderInsTypeCode = "Medicare Conditionally Primary";
                                                    }
                                                    else if (Qlfr_InsuranceTypeCode == "MC")
                                                    {
                                                        oClsRxH_271Master.MailOrderInsTypeCode = "Medicaid";
                                                    }
                                                    else if (Qlfr_InsuranceTypeCode == "MP")
                                                    {
                                                        oClsRxH_271Master.MailOrderInsTypeCode = "Medicare Primary";
                                                    }
                                                    else if (Qlfr_InsuranceTypeCode == "OT")
                                                    {
                                                        oClsRxH_271Master.MailOrderInsTypeCode = "Other (Used for Medicare Part D)";
                                                    }

                                                }
                                                #endregion "Insurance type code - Mail"

                                                oClsRxH_271Master.MailOrdRxDrugCoveragePlanName = oSegment.get_DataElementValue(5);
                                                oClsRxH_271Master.MailOrdRxDrugEligiblityorBenefitInfo = "Out of Pocket (Stop Loss)";
                                                oClsRxH_271Master.MailOrderMonetaryAmount = oSegment.get_DataElementValue(7);
                                               // IsMailOrdRx = true;

                                            }
                                            if (oSegment.get_DataElementValue(3) == "30")//Health Plan Benefit Coverage
                                            {
                                                #region "Insurance type code"

                                                Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);

                                                if (Qlfr_InsuranceTypeCode != "")
                                                {
                                                    switch (Qlfr_InsuranceTypeCode)
                                                    {
                                                        case "47":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                            break;
                                                        case "CP":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Medicare Conditionally Primary";
                                                            break;
                                                        case "MC":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Medicaid";
                                                            break;
                                                        case "MP":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Medicare Primary";
                                                            break;
                                                        case "OT":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Other (Used for Medicare Part D)";
                                                            break;

                                                    }
                                                }

                                                #endregion "Insurance type code"

                                                oClsRxH_271Master.HealthPlanBenefitEligibilityInfo = "Out of Pocket (Stop Loss)";
                                                oClsRxH_271Master.HealthPlanBenefitCoverageName = oSegment.get_DataElementValue(5);//added for ANSI 5010
                                            }

                                            if (oSegment.get_DataElementValue(3) == "")//Specilaity Or LTC Coverage
                                            {
                                                sSpecialtyorLTCCoverageStatus = "Out of Pocket (Stop Loss)";
                                            }
                                        }
                                        #endregion "G - Out Of Pocket (Stop Loss)"

                                        #region "V - Cannot Process"

                                        else if (Qlfr == "V")//Cannot Process
                                        {
                                            //listResponse.Items.Add("Deductibles: " + oSegment.get_DataElementValue(1).Trim());
                                            if (oSegment.get_DataElementValue(3).Trim() == "88")
                                            {
                                                oClsRxH_271Master.IsRetailPharmacyEligible = "NO";
                                                #region "Insurance type code - Retail"
                                                Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
                                                if (Qlfr_InsuranceTypeCode != "")
                                                {
                                                    if (Qlfr_InsuranceTypeCode == "47")
                                                    {
                                                        oClsRxH_271Master.RetailInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                    }
                                                    else if (Qlfr_InsuranceTypeCode == "CP")
                                                    {
                                                        oClsRxH_271Master.RetailInsTypeCode = "Medicare Conditionally Primary";
                                                    }
                                                    else if (Qlfr_InsuranceTypeCode == "MC")
                                                    {
                                                        oClsRxH_271Master.RetailInsTypeCode = "Medicaid";
                                                    }
                                                    else if (Qlfr_InsuranceTypeCode == "MP")
                                                    {
                                                        oClsRxH_271Master.RetailInsTypeCode = "Medicare Primary";
                                                    }
                                                    else if (Qlfr_InsuranceTypeCode == "OT")
                                                    {
                                                        oClsRxH_271Master.RetailInsTypeCode = "Other (Used for Medicare Part D)";
                                                    }

                                                }
                                                #endregion "Insurance type code - Retail"

                                                oClsRxH_271Master.RetailPharmacyCoveragePlanName = oSegment.get_DataElementValue(5);
                                                oClsRxH_271Master.RetailPharmacyEligiblityorBenefitInfo = "Cannot Process";
                                                oClsRxH_271Master.RetailMonetaryAmount = oSegment.get_DataElementValue(7);
                                             //   IsRetailPharmacy = true;

                                            }
                                            if (oSegment.get_DataElementValue(3) == "90")
                                            {
                                                oClsRxH_271Master.IsMailOrdRxDrugEligible = "NO";
                                                #region "Insurance type code - Mail"
                                                Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
                                                if (Qlfr_InsuranceTypeCode != "")
                                                {
                                                    if (Qlfr_InsuranceTypeCode == "47")
                                                    {
                                                        oClsRxH_271Master.MailOrderInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                    }
                                                    else if (Qlfr_InsuranceTypeCode == "CP")
                                                    {
                                                        oClsRxH_271Master.MailOrderInsTypeCode = "Medicare Conditionally Primary";
                                                    }
                                                    else if (Qlfr_InsuranceTypeCode == "MC")
                                                    {
                                                        oClsRxH_271Master.MailOrderInsTypeCode = "Medicaid";
                                                    }
                                                    else if (Qlfr_InsuranceTypeCode == "MP")
                                                    {
                                                        oClsRxH_271Master.MailOrderInsTypeCode = "Medicare Primary";
                                                    }
                                                    else if (Qlfr_InsuranceTypeCode == "OT")
                                                    {
                                                        oClsRxH_271Master.MailOrderInsTypeCode = "Other (Used for Medicare Part D)";
                                                    }

                                                }
                                                #endregion "Insurance type code - Mail"

                                                oClsRxH_271Master.MailOrdRxDrugCoveragePlanName = oSegment.get_DataElementValue(5);
                                                oClsRxH_271Master.MailOrdRxDrugEligiblityorBenefitInfo = "Cannot Process";
                                                oClsRxH_271Master.MailOrderMonetaryAmount = oSegment.get_DataElementValue(7);
                                             //   IsMailOrdRx = true;

                                            }
                                            if (oSegment.get_DataElementValue(3) == "30")//Health Plan Benefit Coverage
                                            {
                                                #region "Insurance type code"

                                                Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);

                                                if (Qlfr_InsuranceTypeCode != "")
                                                {
                                                    switch (Qlfr_InsuranceTypeCode)
                                                    {
                                                        case "47":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                            break;
                                                        case "CP":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Medicare Conditionally Primary";
                                                            break;
                                                        case "MC":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Medicaid";
                                                            break;
                                                        case "MP":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Medicare Primary";
                                                            break;
                                                        case "OT":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Other (Used for Medicare Part D)";
                                                            break;

                                                    }
                                                }

                                                #endregion "Insurance type code"

                                                oClsRxH_271Master.HealthPlanBenefitEligibilityInfo = "Cannot Process";
                                                oClsRxH_271Master.HealthPlanBenefitCoverageName = oSegment.get_DataElementValue(5);//added for ANSI 5010
                                            }
                                        }
                                        #endregion "V - Cannot Process"

                                        #region "I - Not Covered"

                                        else if (Qlfr == "I")//Non-Covered, Added for ANSI 5010
                                        {
                                            //listResponse.Items.Add("Deductibles: " + oSegment.get_DataElementValue(1).Trim());
                                            if (oSegment.get_DataElementValue(3).Trim() == "88")
                                            {
                                                oClsRxH_271Master.IsRetailPharmacyEligible = "NO";
                                                #region "Insurance type code - Retail"

                                                Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);

                                                if (Qlfr_InsuranceTypeCode != "")
                                                {
                                                    switch (Qlfr_InsuranceTypeCode)
                                                    {
                                                        case "47":
                                                            oClsRxH_271Master.RetailInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                            break;
                                                        case "CP":
                                                            oClsRxH_271Master.RetailInsTypeCode = "Medicare Conditionally Primary";
                                                            break;
                                                        case "MC":
                                                            oClsRxH_271Master.RetailInsTypeCode = "Medicaid";
                                                            break;
                                                        case "MP":
                                                            oClsRxH_271Master.RetailInsTypeCode = "Medicare Primary";
                                                            break;
                                                        case "OT":
                                                            oClsRxH_271Master.RetailInsTypeCode = "Other (Used for Medicare Part D)";
                                                            break;


                                                    }

                                                }
                                                #endregion "Insurance type code - Retail"
                                                oClsRxH_271Master.RetailPharmacyCoveragePlanName = oSegment.get_DataElementValue(5);
                                                oClsRxH_271Master.RetailPharmacyEligiblityorBenefitInfo = "Non-Covered";
                                                oClsRxH_271Master.RetailMonetaryAmount = oSegment.get_DataElementValue(7);
                                          //      IsRetailPharmacy = true;

                                            }
                                            if (oSegment.get_DataElementValue(3) == "90")
                                            {
                                                oClsRxH_271Master.IsMailOrdRxDrugEligible = "NO";
                                                #region "Insurance type code - Mail"
                                                Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
                                                if (Qlfr_InsuranceTypeCode != "")
                                                {
                                                    if (Qlfr_InsuranceTypeCode == "47")
                                                    {
                                                        oClsRxH_271Master.MailOrderInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                    }
                                                    else if (Qlfr_InsuranceTypeCode == "CP")
                                                    {
                                                        oClsRxH_271Master.MailOrderInsTypeCode = "Medicare Conditionally Primary";
                                                    }
                                                    else if (Qlfr_InsuranceTypeCode == "MC")
                                                    {
                                                        oClsRxH_271Master.MailOrderInsTypeCode = "Medicaid";
                                                    }
                                                    else if (Qlfr_InsuranceTypeCode == "MP")
                                                    {
                                                        oClsRxH_271Master.MailOrderInsTypeCode = "Medicare Primary";
                                                    }
                                                    else if (Qlfr_InsuranceTypeCode == "OT")
                                                    {
                                                        oClsRxH_271Master.MailOrderInsTypeCode = "Other (Used for Medicare Part D)";
                                                    }

                                                }
                                                #endregion "Insurance type code - Mail"
                                                oClsRxH_271Master.MailOrdRxDrugCoveragePlanName = oSegment.get_DataElementValue(5);
                                                oClsRxH_271Master.MailOrdRxDrugEligiblityorBenefitInfo = "Non-Covered";
                                                oClsRxH_271Master.MailOrderMonetaryAmount = oSegment.get_DataElementValue(7);
                                             //   IsMailOrdRx = true;

                                            }
                                            if (oSegment.get_DataElementValue(3) == "30")//Health Plan Benefit Coverage
                                            {
                                                #region "Insurance type code"

                                                Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);

                                                if (Qlfr_InsuranceTypeCode != "")
                                                {
                                                    switch (Qlfr_InsuranceTypeCode)
                                                    {
                                                        case "47":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                            break;
                                                        case "CP":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Medicare Conditionally Primary";
                                                            break;
                                                        case "MC":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Medicaid";
                                                            break;
                                                        case "MP":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Medicare Primary";
                                                            break;
                                                        case "OT":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Other (Used for Medicare Part D)";
                                                            break;

                                                    }
                                                }

                                                #endregion "Insurance type code"

                                                oClsRxH_271Master.HealthPlanBenefitEligibilityInfo = "Non-Covered";
                                                oClsRxH_271Master.HealthPlanBenefitCoverageName = oSegment.get_DataElementValue(5);//added for ANSI 5010
                                            }

                                            if (oSegment.get_DataElementValue(3) == "")//Specilaity Or LTC Coverage
                                            {
                                                sSpecialtyorLTCCoverageStatus = "Non-Covered";
                                            }

                                        }

                                        #endregion "I - Not Covered"
                                        #endregion " Subscriber EB Segment (When NM1 is IL) "
                                    }//IF loop end for Subscriber EB Segment
                                    else
                                    {
                                        if (blnContractedProvider == true)
                                        {
                                            #region " Contracted Provider EB Segment (When NM1 is 13) "
                                            if (Qlfr == "1")//Active Coverage
                                            {
                                                //listResponse.Items.Add("Active Coverage: " + oSegment.get_DataElementValue(3).Trim());
                                                if (oSegment.get_DataElementValue(3) == "88")
                                                {
                                                    oClsRxH_271Master.ContProvRetailsEligible = "Yes";
                                                    oClsRxH_271Master.ContProvRetailCoverageInfo = "Active Coverage";
                                                    //Mail ord insurance type code value
                                                    #region "Insurance type code - Retail"
                                                    Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
                                                    if (Qlfr_InsuranceTypeCode != "")
                                                    {
                                                        if (Qlfr_InsuranceTypeCode == "47")
                                                        {
                                                            oClsRxH_271Master.ContProvRetailInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "CP")
                                                        {
                                                            oClsRxH_271Master.ContProvRetailInsTypeCode = "Medicare Conditionally Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MC")
                                                        {
                                                            oClsRxH_271Master.ContProvRetailInsTypeCode = "Medicaid";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MP")
                                                        {
                                                            oClsRxH_271Master.ContProvRetailInsTypeCode = "Medicare Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "OT")
                                                        {
                                                            oClsRxH_271Master.ContProvRetailInsTypeCode = "Other (Used for Medicare Part D)";
                                                        }

                                                    }
                                                    #endregion "Insurance type code - Retail"
                                                    // oClsRxH_271Master.ContProvRetailInsTypeCode = oSegment.get_DataElementValue(4);
                                                    //Mail ord Monetary amount
                                                    oClsRxH_271Master.ContProvRetailMonetaryAmt = oSegment.get_DataElementValue(7);

                                                }
                                                if (oSegment.get_DataElementValue(3) == "90")
                                                {
                                                    oClsRxH_271Master.ContProvMailOrderEligible = "Yes";
                                                    oClsRxH_271Master.ContProvMailOrderCoverageInfo = "Active Coverage";
                                                    //Mail ord insurance type code value
                                                    #region "Insurance type code - Mail"
                                                    Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
                                                    if (Qlfr_InsuranceTypeCode != "")
                                                    {
                                                        if (Qlfr_InsuranceTypeCode == "47")
                                                        {
                                                            oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "CP")
                                                        {
                                                            oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Medicare Conditionally Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MC")
                                                        {
                                                            oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Medicaid";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MP")
                                                        {
                                                            oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Medicare Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "OT")
                                                        {
                                                            oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Other (Used for Medicare Part D)";
                                                        }

                                                    }
                                                    #endregion "Insurance type code - Mail"
                                                    //oClsRxH_271Master.ContProvMailOrderInsTypeCode = oSegment.get_DataElementValue(4);
                                                    //Mail ord Monetary amount
                                                    oClsRxH_271Master.ContProvMailOrderMonetaryAmt = oSegment.get_DataElementValue(7);
                                                }
                                            }

                                            else if (Qlfr == "6")//Inactive
                                            {
                                                //listResponse.Items.Add("Co-Payment: " + oSegment.get_DataElementValue(1).Trim());
                                                if (oSegment.get_DataElementValue(3).Trim() == "88")
                                                {

                                                    oClsRxH_271Master.ContProvRetailsEligible = "NO";
                                                    oClsRxH_271Master.ContProvRetailCoverageInfo = "Inactive";
                                                    //Mail ord insurance type code value
                                                    #region "Insurance type code - Retail"
                                                    Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
                                                    if (Qlfr_InsuranceTypeCode != "")
                                                    {
                                                        if (Qlfr_InsuranceTypeCode == "47")
                                                        {
                                                            oClsRxH_271Master.ContProvRetailInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "CP")
                                                        {
                                                            oClsRxH_271Master.ContProvRetailInsTypeCode = "Medicare Conditionally Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MC")
                                                        {
                                                            oClsRxH_271Master.ContProvRetailInsTypeCode = "Medicaid";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MP")
                                                        {
                                                            oClsRxH_271Master.ContProvRetailInsTypeCode = "Medicare Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "OT")
                                                        {
                                                            oClsRxH_271Master.ContProvRetailInsTypeCode = "Other (Used for Medicare Part D)";
                                                        }

                                                    }
                                                    #endregion "Insurance type code - Retail"
                                                    //oClsRxH_271Master.ContProvRetailInsTypeCode = oSegment.get_DataElementValue(4);
                                                    //Mail ord Monetary amount
                                                    oClsRxH_271Master.ContProvRetailMonetaryAmt = oSegment.get_DataElementValue(7);
                                                }
                                                if (oSegment.get_DataElementValue(3) == "90")
                                                {
                                                    oClsRxH_271Master.ContProvMailOrderEligible = "NO";
                                                    oClsRxH_271Master.ContProvMailOrderCoverageInfo = "Inactive";
                                                    //Mail ord insurance type code value
                                                    #region "Insurance type code - Mail"
                                                    Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
                                                    if (Qlfr_InsuranceTypeCode != "")
                                                    {
                                                        if (Qlfr_InsuranceTypeCode == "47")
                                                        {
                                                            oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "CP")
                                                        {
                                                            oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Medicare Conditionally Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MC")
                                                        {
                                                            oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Medicaid";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MP")
                                                        {
                                                            oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Medicare Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "OT")
                                                        {
                                                            oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Other (Used for Medicare Part D)";
                                                        }

                                                    }
                                                    #endregion "Insurance type code - Mail"
                                                    //oClsRxH_271Master.ContProvMailOrderInsTypeCode = oSegment.get_DataElementValue(4);
                                                    //Mail ord Monetary amount
                                                    oClsRxH_271Master.ContProvMailOrderMonetaryAmt = oSegment.get_DataElementValue(7);

                                                }
                                            }
                                            else if (Qlfr == "G")//Out Of Pocket (Stop Loss)
                                            {
                                                //listResponse.Items.Add("Deductibles: " + oSegment.get_DataElementValue(1).Trim());
                                                if (oSegment.get_DataElementValue(3).Trim() == "88")
                                                {

                                                    oClsRxH_271Master.ContProvRetailsEligible = "NO";
                                                    oClsRxH_271Master.ContProvRetailCoverageInfo = "Out of Pocket (Stop Loss)";
                                                    //Mail ord insurance type code value
                                                    #region "Insurance type code - Retail"
                                                    Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
                                                    if (Qlfr_InsuranceTypeCode != "")
                                                    {
                                                        if (Qlfr_InsuranceTypeCode == "47")
                                                        {
                                                            oClsRxH_271Master.ContProvRetailInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "CP")
                                                        {
                                                            oClsRxH_271Master.ContProvRetailInsTypeCode = "Medicare Conditionally Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MC")
                                                        {
                                                            oClsRxH_271Master.ContProvRetailInsTypeCode = "Medicaid";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MP")
                                                        {
                                                            oClsRxH_271Master.ContProvRetailInsTypeCode = "Medicare Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "OT")
                                                        {
                                                            oClsRxH_271Master.ContProvRetailInsTypeCode = "Other (Used for Medicare Part D)";
                                                        }

                                                    }
                                                    #endregion "Insurance type code - Retail"
                                                    //oClsRxH_271Master.ContProvRetailInsTypeCode = oSegment.get_DataElementValue(4);
                                                    //Mail ord Monetary amount
                                                    oClsRxH_271Master.ContProvRetailMonetaryAmt = oSegment.get_DataElementValue(7);
                                                }
                                                if (oSegment.get_DataElementValue(3) == "90")
                                                {

                                                    oClsRxH_271Master.ContProvMailOrderEligible = "NO";
                                                    oClsRxH_271Master.ContProvMailOrderCoverageInfo = "Out of Pocket (Stop Loss)";
                                                    //Mail ord insurance type code value
                                                    #region "Insurance type code - Mail"
                                                    Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
                                                    if (Qlfr_InsuranceTypeCode != "")
                                                    {
                                                        if (Qlfr_InsuranceTypeCode == "47")
                                                        {
                                                            oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "CP")
                                                        {
                                                            oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Medicare Conditionally Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MC")
                                                        {
                                                            oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Medicaid";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MP")
                                                        {
                                                            oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Medicare Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "OT")
                                                        {
                                                            oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Other (Used for Medicare Part D)";
                                                        }

                                                    }
                                                    #endregion "Insurance type code - Mail"
                                                    //oClsRxH_271Master.ContProvMailOrderInsTypeCode = oSegment.get_DataElementValue(4);
                                                    //Mail ord Monetary amount
                                                    oClsRxH_271Master.ContProvMailOrderMonetaryAmt = oSegment.get_DataElementValue(7);

                                                }
                                            }
                                            else if (Qlfr == "V")//Cannot Process
                                            {
                                                //listResponse.Items.Add("Deductibles: " + oSegment.get_DataElementValue(1).Trim());
                                                if (oSegment.get_DataElementValue(3).Trim() == "88")
                                                {
                                                    oClsRxH_271Master.ContProvRetailsEligible = "NO";
                                                    oClsRxH_271Master.ContProvRetailCoverageInfo = "Cannot Process";
                                                    //Mail ord insurance type code value
                                                    #region "Insurance type code - Retail"
                                                    Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
                                                    if (Qlfr_InsuranceTypeCode != "")
                                                    {
                                                        if (Qlfr_InsuranceTypeCode == "47")
                                                        {
                                                            oClsRxH_271Master.ContProvRetailInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "CP")
                                                        {
                                                            oClsRxH_271Master.ContProvRetailInsTypeCode = "Medicare Conditionally Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MC")
                                                        {
                                                            oClsRxH_271Master.ContProvRetailInsTypeCode = "Medicaid";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MP")
                                                        {
                                                            oClsRxH_271Master.ContProvRetailInsTypeCode = "Medicare Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "OT")
                                                        {
                                                            oClsRxH_271Master.ContProvRetailInsTypeCode = "Other (Used for Medicare Part D)";
                                                        }

                                                    }
                                                    #endregion "Insurance type code - Retail"
                                                    //oClsRxH_271Master.ContProvRetailInsTypeCode = oSegment.get_DataElementValue(4);
                                                    //Mail ord Monetary amount
                                                    oClsRxH_271Master.ContProvRetailMonetaryAmt = oSegment.get_DataElementValue(7);
                                                }
                                                if (oSegment.get_DataElementValue(3) == "90")
                                                {
                                                    oClsRxH_271Master.ContProvMailOrderEligible = "NO";
                                                    oClsRxH_271Master.ContProvMailOrderCoverageInfo = "Cannot Process";
                                                    //Mail ord insurance type code value
                                                    #region "Insurance type code - Mail"
                                                    Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
                                                    if (Qlfr_InsuranceTypeCode != "")
                                                    {
                                                        if (Qlfr_InsuranceTypeCode == "47")
                                                        {
                                                            oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "CP")
                                                        {
                                                            oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Medicare Conditionally Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MC")
                                                        {
                                                            oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Medicaid";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MP")
                                                        {
                                                            oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Medicare Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "OT")
                                                        {
                                                            oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Other (Used for Medicare Part D)";
                                                        }

                                                    }
                                                    #endregion "Insurance type code - Mail"
                                                    //oClsRxH_271Master.ContProvMailOrderInsTypeCode = oSegment.get_DataElementValue(4);
                                                    //Mail ord Monetary amount
                                                    oClsRxH_271Master.ContProvMailOrderMonetaryAmt = oSegment.get_DataElementValue(7);

                                                }
                                            }
                                            #endregion " Contracted Provider EB Segment (When NM1 is 13) "
                                        }// IF loop end for Contracted provider EB Segment
                                        else if (blnPrimaryPayer == true)
                                        {
                                            #region " Primary Payer EB Segment (When NM1 is PRP) "
                                            if (Qlfr == "1")//Active Coverage
                                            {
                                                //listResponse.Items.Add("Active Coverage: " + oSegment.get_DataElementValue(3).Trim());
                                                if (oSegment.get_DataElementValue(3) == "88")
                                                {

                                                    oClsRxH_271Master.PrimaryPayerRetailsEligible = "Yes";
                                                    oClsRxH_271Master.PrimaryPayerRetailCoverageInfo = "Active Coverage";
                                                    //oClsRxH_271Master.MailOrdRxDrugCoveragePlanName = oSegment.get_DataElementValue(5);
                                                    #region "Insurance type code - Retail"
                                                    Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
                                                    if (Qlfr_InsuranceTypeCode != "")
                                                    {
                                                        if (Qlfr_InsuranceTypeCode == "47")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "CP")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Medicare Conditionally Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MC")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Medicaid";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MP")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Medicare Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "OT")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Other (Used for Medicare Part D)";
                                                        }

                                                    }
                                                    #endregion "Insurance type code - Retail"
                                                    //oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = oSegment.get_DataElementValue(4);
                                                    oClsRxH_271Master.PrimaryPayerRetailMonetaryAmt = oSegment.get_DataElementValue(7);

                                                }
                                                if (oSegment.get_DataElementValue(3) == "90")
                                                {
                                                    oClsRxH_271Master.PrimaryPayerMailOrderEligible = "Yes";
                                                    oClsRxH_271Master.PrimaryPayerMailOrderCoverageInfo = "Active Coverage";
                                                    #region "Insurance type code - Mail"
                                                    Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
                                                    if (Qlfr_InsuranceTypeCode != "")
                                                    {
                                                        if (Qlfr_InsuranceTypeCode == "47")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "CP")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Medicare Conditionally Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MC")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Medicaid";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MP")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Medicare Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "OT")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Other (Used for Medicare Part D)";
                                                        }

                                                    }
                                                    #endregion "Insurance type code - Mail"
                                                    //oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = oSegment.get_DataElementValue(4);
                                                    oClsRxH_271Master.PrimaryPayerMailOrderMonetaryAmt = oSegment.get_DataElementValue(7);

                                                }
                                            }

                                            else if (Qlfr == "6")//Inactive
                                            {
                                                //listResponse.Items.Add("Co-Payment: " + oSegment.get_DataElementValue(1).Trim());
                                                if (oSegment.get_DataElementValue(3).Trim() == "88")
                                                {
                                                    oClsRxH_271Master.PrimaryPayerRetailsEligible = "NO";
                                                    oClsRxH_271Master.PrimaryPayerRetailCoverageInfo = "Inactive";
                                                    //oClsRxH_271Master.MailOrdRxDrugCoveragePlanName = oSegment.get_DataElementValue(5);
                                                    #region "Insurance type code - Retail"
                                                    Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
                                                    if (Qlfr_InsuranceTypeCode != "")
                                                    {
                                                        if (Qlfr_InsuranceTypeCode == "47")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "CP")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Medicare Conditionally Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MC")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Medicaid";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MP")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Medicare Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "OT")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Other (Used for Medicare Part D)";
                                                        }

                                                    }
                                                    #endregion "Insurance type code - Retail"
                                                    //oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = oSegment.get_DataElementValue(4);
                                                    oClsRxH_271Master.PrimaryPayerRetailMonetaryAmt = oSegment.get_DataElementValue(7);

                                                }
                                                if (oSegment.get_DataElementValue(3) == "90")
                                                {
                                                    oClsRxH_271Master.PrimaryPayerMailOrderEligible = "NO";
                                                    oClsRxH_271Master.PrimaryPayerMailOrderCoverageInfo = "Inactive";
                                                    #region "Insurance type code - Mail"
                                                    Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
                                                    if (Qlfr_InsuranceTypeCode != "")
                                                    {
                                                        if (Qlfr_InsuranceTypeCode == "47")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "CP")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Medicare Conditionally Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MC")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Medicaid";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MP")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Medicare Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "OT")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Other (Used for Medicare Part D)";
                                                        }

                                                    }
                                                    #endregion "Insurance type code - Mail"
                                                    oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = oSegment.get_DataElementValue(4);
                                                    oClsRxH_271Master.PrimaryPayerMailOrderMonetaryAmt = oSegment.get_DataElementValue(7);

                                                }
                                            }
                                            else if (Qlfr == "G")//Out Of Pocket (Stop Loss)
                                            {
                                                //listResponse.Items.Add("Deductibles: " + oSegment.get_DataElementValue(1).Trim());
                                                if (oSegment.get_DataElementValue(3).Trim() == "88")
                                                {
                                                    oClsRxH_271Master.PrimaryPayerRetailsEligible = "NO";
                                                    oClsRxH_271Master.PrimaryPayerRetailCoverageInfo = "Out of Pocket (Stop Loss)";
                                                    //oClsRxH_271Master.MailOrdRxDrugCoveragePlanName = oSegment.get_DataElementValue(5);
                                                    #region "Insurance type code - Retail"
                                                    Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
                                                    if (Qlfr_InsuranceTypeCode != "")
                                                    {
                                                        if (Qlfr_InsuranceTypeCode == "47")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "CP")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Medicare Conditionally Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MC")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Medicaid";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MP")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Medicare Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "OT")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Other (Used for Medicare Part D)";
                                                        }

                                                    }
                                                    #endregion "Insurance type code - Retail"
                                                    //oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = oSegment.get_DataElementValue(4);
                                                    oClsRxH_271Master.PrimaryPayerRetailMonetaryAmt = oSegment.get_DataElementValue(7);

                                                }
                                                if (oSegment.get_DataElementValue(3) == "90")
                                                {
                                                    oClsRxH_271Master.PrimaryPayerMailOrderEligible = "NO";
                                                    oClsRxH_271Master.PrimaryPayerMailOrderCoverageInfo = "Out of Pocket (Stop Loss)";
                                                    #region "Insurance type code - Mail"
                                                    Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
                                                    if (Qlfr_InsuranceTypeCode != "")
                                                    {
                                                        if (Qlfr_InsuranceTypeCode == "47")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "CP")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Medicare Conditionally Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MC")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Medicaid";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MP")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Medicare Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "OT")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Other (Used for Medicare Part D)";
                                                        }

                                                    }
                                                    #endregion "Insurance type code - Mail"
                                                    //oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = oSegment.get_DataElementValue(4);
                                                    oClsRxH_271Master.PrimaryPayerMailOrderMonetaryAmt = oSegment.get_DataElementValue(7);

                                                }
                                            }
                                            else if (Qlfr == "V")//Cannot Process
                                            {
                                                //listResponse.Items.Add("Deductibles: " + oSegment.get_DataElementValue(1).Trim());
                                                if (oSegment.get_DataElementValue(3).Trim() == "88")
                                                {
                                                    oClsRxH_271Master.PrimaryPayerRetailsEligible = "NO";
                                                    oClsRxH_271Master.PrimaryPayerRetailCoverageInfo = "Cannot Process";
                                                    //oClsRxH_271Master.MailOrdRxDrugCoveragePlanName = oSegment.get_DataElementValue(5);
                                                    #region "Insurance type code - Retail"
                                                    Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
                                                    if (Qlfr_InsuranceTypeCode != "")
                                                    {
                                                        if (Qlfr_InsuranceTypeCode == "47")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "CP")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Medicare Conditionally Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MC")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Medicaid";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MP")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Medicare Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "OT")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Other (Used for Medicare Part D)";
                                                        }

                                                    }
                                                    #endregion "Insurance type code - Retail"
                                                    //oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = oSegment.get_DataElementValue(4);
                                                    oClsRxH_271Master.PrimaryPayerRetailMonetaryAmt = oSegment.get_DataElementValue(7);

                                                }
                                                if (oSegment.get_DataElementValue(3) == "90")
                                                {
                                                    oClsRxH_271Master.PrimaryPayerMailOrderEligible = "NO";
                                                    oClsRxH_271Master.PrimaryPayerMailOrderCoverageInfo = "Cannot Process";
                                                    #region "Insurance type code - Mail"
                                                    Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
                                                    if (Qlfr_InsuranceTypeCode != "")
                                                    {
                                                        if (Qlfr_InsuranceTypeCode == "47")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "CP")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Medicare Conditionally Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MC")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Medicaid";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MP")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Medicare Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "OT")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Other (Used for Medicare Part D)";
                                                        }

                                                    }
                                                    #endregion "Insurance type code - Mail"
                                                    //oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = oSegment.get_DataElementValue(4);
                                                    oClsRxH_271Master.PrimaryPayerMailOrderMonetaryAmt = oSegment.get_DataElementValue(7);

                                                }
                                            }
                                            #endregion " PRP EB Segment"
                                        }//IF Loop end for Primary Payer EB Segment


                                    }



                                }

                                else if (sSegmentID == "AAA")
                                {
                                    //sValue.Append(oSegment.get_DataElementValue(2) + Environment.NewLine);
                                    if (oSegment.get_DataElementValue(3).Trim() != "")
                                    {
                                        //listResponse.Items.Add("Eligibility Rejection Reason: " + GetSubscriberRejectionReason(oSegment.get_DataElementValue(3)));
                                        //listResponse.Items.Add("Eligibility Follow up: " + GetSubscriberFollowUp(oSegment.get_DataElementValue(4)));
                                    }

                                    //EDIReturnResult = oSegment.get_DataElementValue(3).Trim() + "-" + oSegment.get_DataElementValue(4).Trim();
                                    //AddNote(_PatientId, EDIReturnResult);
                                }

                                #region "Subscriber Additional Information - PLAN ID/ GROUP NUMBER/ ALTERNATE LIST ID/ COVERAGE LIST ID/ DRUG FORMULARY NUMBER ID/ INSURANCE POLICY NUMBER(*COPAY ID)/ PLAN NETWORK ID (*BIN/PCN)"

                                else if (sSegmentID == "REF")//Added for ANSI 5010 [refer Prescription Benefit IG 2011-04-15.pdf - pg 122]
                                {
                                    Qlfr = oSegment.get_DataElementValue(1);

                                    switch (Qlfr)
                                    {
                                        case "18"://Plan ID
                                            oClsRxH_271Master.HealthPlanNumber = oSegment.get_DataElementValue(2);
                                            oClsRxH_271Master.HealthPlanName = oSegment.get_DataElementValue(3);//health plan name in the REF03 
                                            break;
                                        case "6P"://Group Number
                                            oClsRxH_271Master.GroupId = oSegment.get_DataElementValue(2);//group ID in the REF02 will be used to populate drug history request and new prescription transactions.
                                            oClsRxH_271Master.GroupName = oSegment.get_DataElementValue(3);//group ID in the REF02 will be used to populate drug history request and new prescription transactions.
                                            break;
                                        case "ALS":// Alternate List ID
                                            oClsRxH_271Master.AlternativeListId = oSegment.get_DataElementValue(2);
                                            break;
                                        case "CLI"://Coverage List ID
                                            oClsRxH_271Master.CoverageId = oSegment.get_DataElementValue(2);
                                            break;
                                        case "FO"://Drug Formulary Number ID
                                            oClsRxH_271Master.FormularyListId = oSegment.get_DataElementValue(2);
                                            break;
                                        case "IG"://Insurance Policy Number (*Copay ID)
                                            oClsRxH_271Master.CopayId = oSegment.get_DataElementValue(2);
                                            break;
                                        case "N6"://Plan Network ID (*BIN/PCN)
                                            string BinNumber = oSegment.get_DataElementValue(2);
                                            oClsRxH_271Master.BINNumber = oSegment.get_DataElementValue(2);
                                            string sPCNNumber = "";
                                            sPCNNumber = oSegment.get_DataElementValue(3);//need to add property procedure to read the PCN number and to user we will show as BIN/PCN = 234876/AV [David Cross]
                                            if (sPCNNumber != "")
                                            {
                                                if (BinNumber != "")
                                                {
                                                    oClsRxH_271Master.BINNumber = BinNumber + " / " + sPCNNumber;
                                                }
                                                else
                                                {
                                                    oClsRxH_271Master.BINNumber = sPCNNumber;
                                                }
                                            }
                                            else
                                            {
                                                if (BinNumber != "")
                                                {
                                                    oClsRxH_271Master.BINNumber = BinNumber;
                                                }
                                            }
                                            break;

                                    }
                                }

                                #endregion "Subscriber Additional Information - PLAN ID/ GROUP NUMBER/ ALTERNATE LIST ID/ COVERAGE LIST ID/ DRUG FORMULARY NUMBER ID/ INSURANCE POLICY NUMBER(*COPAY ID)/ PLAN NETWORK ID (*BIN/PCN)"

                                #region "DTP - Subscriber Date"
                                else if (sSegmentID == "DTP")
                                {
                                    //Added for ANSI 5010
                                    #region "Pharmacy Eligibility date/service date according to pharmacy type"

                                    switch (Qlfr_PharmacyCode)
                                    {
                                        case "30"://Health Plan Benefit Coverage 
                                            Qlfr = oSegment.get_DataElementValue(2);
                                            if (Qlfr == "D8")//Eligiblity Range of dates when the subscriber or dependent were eligible for benefits
                                            {
                                                oClsRxH_271Master.HlthPlnBenftCovEligibilityDate = oSegment.get_DataElementValue(3);//commented for ANSI 5010
                                            }
                                            else if (Qlfr == "RD8")//Service (Recommended by RxHub) Begin and end dates of the service being rendered
                                            {
                                                oClsRxH_271Master.HlthPlnBenftCovServiceDate = oSegment.get_DataElementValue(3);//commented for ANSI 5010
                                            }

                                            break;
                                        case "88"://Retail Order Pharmacy
                                            Qlfr = oSegment.get_DataElementValue(2);
                                            if (Qlfr == "D8")//Eligiblity Range of dates when the subscriber or dependent were eligible for benefits
                                            {
                                                oClsRxH_271Master.RetailOrdPhrmEligibilityDate = oSegment.get_DataElementValue(3);//commented for ANSI 5010
                                            }
                                            else if (Qlfr == "RD8")//Service (Recommended by RxHub) Begin and end dates of the service being rendered
                                            {
                                                oClsRxH_271Master.RetailPhrmServiceDate = oSegment.get_DataElementValue(3);//commented for ANSI 5010
                                            }

                                            break;
                                        case "90"://Mail Order Pharmacy
                                            Qlfr = oSegment.get_DataElementValue(2);
                                            if (Qlfr == "D8")//Eligiblity Range of dates when the subscriber or dependent were eligible for benefits
                                            {
                                                oClsRxH_271Master.MailOrdPhrmEligibilityDate = oSegment.get_DataElementValue(3);//commented for ANSI 5010
                                            }
                                            else if (Qlfr == "RD8")//Service (Recommended by RxHub) Begin and end dates of the service being rendered
                                            {
                                                oClsRxH_271Master.MailOrdPhrmServiceDate = oSegment.get_DataElementValue(3);//commented for ANSI 5010
                                            }
                                            break;
                                        case ""://
                                            Qlfr = oSegment.get_DataElementValue(2);
                                            if (Qlfr == "D8")//Eligiblity Range of dates when the subscriber or dependent were eligible for benefits
                                            {
                                                SpecialtyorLTCPhrmEligibilityDate = oSegment.get_DataElementValue(3);//Add new property proc

                                            }
                                            else if (Qlfr == "RD8")//Service (Recommended by RxHub) Begin and end dates of the service being rendered
                                            {
                                                SpecialtyorLTCPhrmServiceDate = oSegment.get_DataElementValue(3);//Add new property proc

                                            }
                                            break;

                                    }
                                    #endregion "Pharmacy Eligibility date/service date according to pharmacy type"
                                    //Commented for Added for ANSI 5010
                                    //Qlfr = oSegment.get_DataElementValue(2);
                                    //if (Qlfr == "D8")//Eligiblity Range of dates when the subscriber or dependent were eligible for benefits
                                    //{
                                    //    //oClsRxH_271Master.EligiblityDate = oSegment.get_DataElementValue(3);//commented for ANSI 5010
                                    //}
                                    //else if (Qlfr == "RD8")//Service (Recommended by RxHub) Begin and end dates of the service being rendered
                                    //{
                                    //    //oClsRxH_271Master.ServiceDate = oSegment.get_DataElementValue(3);//commented for ANSI 5010
                                    //}

                                    //sValue.Append("Subscriber Service : " + oSegment.get_DataElementValue(1) + Environment.NewLine);
                                    //sValue.Append("Subscriber Date : " + oSegment.get_DataElementValue(2) + Environment.NewLine);
                                    //sValue.Append("Subscriber Service Date : " + oSegment.get_DataElementValue(3) + Environment.NewLine);

                                }
                                #endregion "DTP - Subscriber Date"

                                //Added from ANSI 5010
                                #region "MSG - Message Text"
                                else if (sSegmentID == "MSG")
                                {
                                    Qlfr_MSGText = oSegment.get_DataElementValue(1);
                                    switch (Qlfr_MSGText)
                                    {
                                        case "LTC":
                                            oClsRxH_271Master.LTCPhEligiblityorBenefitInfo = sSpecialtyorLTCCoverageStatus;
                                            oClsRxH_271Master.LTCPharmCovName = sSpecialtyorLTCPhrmCovName;
                                            oClsRxH_271Master.LTCPhrmEligDate = SpecialtyorLTCPhrmEligibilityDate;
                                            oClsRxH_271Master.LTCPhrmServiceDate = SpecialtyorLTCPhrmServiceDate;
                                            break;
                                        case "LTC PHARMACY":
                                            oClsRxH_271Master.LTCPhEligiblityorBenefitInfo = sSpecialtyorLTCCoverageStatus;
                                            oClsRxH_271Master.LTCPharmCovName = sSpecialtyorLTCPhrmCovName;
                                            oClsRxH_271Master.LTCPhrmEligDate = SpecialtyorLTCPhrmEligibilityDate;
                                            oClsRxH_271Master.LTCPhrmServiceDate = SpecialtyorLTCPhrmServiceDate;
                                            break;
                                        case "SPECIALTY":
                                            oClsRxH_271Master.SpecialityPhEligiblityorBenefitInfo = sSpecialtyorLTCCoverageStatus;
                                            oClsRxH_271Master.SpecialtyPharmCovName = sSpecialtyorLTCPhrmCovName;
                                            oClsRxH_271Master.SpecialtyPhrmEligDate = SpecialtyorLTCPhrmEligibilityDate;
                                            oClsRxH_271Master.SpecialtyPhrmServiceDate = SpecialtyorLTCPhrmServiceDate;
                                            break;
                                        case "SPECIALTY PHARMACY":
                                            oClsRxH_271Master.SpecialityPhEligiblityorBenefitInfo = sSpecialtyorLTCCoverageStatus;
                                            oClsRxH_271Master.SpecialtyPharmCovName = sSpecialtyorLTCPhrmCovName;
                                            oClsRxH_271Master.SpecialtyPhrmEligDate = SpecialtyorLTCPhrmEligibilityDate;
                                            oClsRxH_271Master.SpecialtyPhrmServiceDate = SpecialtyorLTCPhrmServiceDate;
                                            break;
                                    }
                                }
                                #endregion "MSG - Message Text"



                            }

                            #endregion "EB - Subscriber Eligibility or Benefit Information"

                            else if (sLoopSection == "HL;NM1;EB;III")
                            {
                                if (sSegmentID == "III")
                                {

                                }
                                if (sSegmentID == "NM1")
                                {

                                }
                            }

                            else if (sLoopSection == "HL;NM1;EB;NM1")
                            {
                                if (sSegmentID == "NM1")
                                {
                                    blnNM1Found = true;
                                }

                                //read the Contracted Provider and Primary Payer information in this section
                                if (sSegmentID == "NM1")
                                {
                                    //the segment value 2 will return the value 13/PRP. if 13 insert the isContracted value as Yes/NO 
                                    //if the value is PRP make the  isPrimaryPayer value as Yes/NO

                                    //Contracted Provider
                                    if (oSegment.get_DataElementValue(1) == "13")
                                    {
                                        blnContractedProvider = true;
                                        oClsRxH_271Master.IsContractedProvider = "Yes";
                                        //read the Name 
                                        oClsRxH_271Master.ContractedProviderName = oSegment.get_DataElementValue(3);
                                        //Number
                                        oClsRxH_271Master.ContractedProviderNumber = oSegment.get_DataElementValue(9);

                                    }

                                    //Primary Payer
                                    else if (oSegment.get_DataElementValue(1) == "PRP")
                                    {
                                        blnPrimaryPayer = true;
                                        blnContractedProvider = false;
                                        oClsRxH_271Master.IsPrimaryPayer = "Yes";
                                        //read the Name 
                                        oClsRxH_271Master.PrimaryPayerName = oSegment.get_DataElementValue(3);
                                        //Number
                                        oClsRxH_271Master.PrimaryPayerNumber = oSegment.get_DataElementValue(9);

                                        //read the EB loop present in this section


                                    }

                                }


                            }
                        }

                        #endregion "Subscriber Level"

                        #region "Dependant Level"

                        //*************************** Depandant Loop
                        else if (sEntity == "23")
                        {
                            if (sLoopSection == "HL")
                            {
                                if (sSegmentID == "HL")
                                {
                                    //sValue.Append(oSegment.get_DataElementValue(1) + Environment.NewLine);
                                    //sValue.Append(oSegment.get_DataElementValue(2) + Environment.NewLine);
                                    //sValue.Append(oSegment.get_DataElementValue(3) + Environment.NewLine);
                                }
                                else if (sSegmentID == "TRN")
                                {
                                    //sValue.Append(oSegment.get_DataElementValue(1) + Environment.NewLine);
                                    //sValue.Append(oSegment.get_DataElementValue(2) + Environment.NewLine);
                                    //sValue.Append(oSegment.get_DataElementValue(3) + Environment.NewLine);
                                }
                            }

                            else if (sLoopSection == "HL;NM1")
                            {
                                if (sSegmentID == "NM1")
                                {

                                    //sValue.Append("Subscriber Last Name : " + oSegment.get_DataElementValue(3) + Environment.NewLine);
                                    //sValue.Append("Subscriber First Name : " + oSegment.get_DataElementValue(4) + Environment.NewLine);
                                    //sValue.Append("Subscriber ID: " + oSegment.get_DataElementValue(9) + Environment.NewLine);

                                    oClsRxH_271Master.DLastName = oSegment.get_DataElementValue(3);//Dependant Last Name

                                    oClsRxH_271Master.DFirstName = oSegment.get_DataElementValue(4);//Dependant First Name

                                    oClsRxH_271Master.DMiddleName = oSegment.get_DataElementValue(5);//Dependant Middle Name

                                }
                                else if (sSegmentID == "N3")
                                {
                                    //sValue.Append("Subscriber Address : " + oSegment.get_DataElementValue(1) + Environment.NewLine);

                                    oClsRxH_271Master.DAddress1 = oSegment.get_DataElementValue(1);//Dependant Address line1

                                    oClsRxH_271Master.DAddress2 = oSegment.get_DataElementValue(2);//Dependant Address line2

                                }
                                else if (sSegmentID == "N4")
                                {

                                    oClsRxH_271Master.DCity = oSegment.get_DataElementValue(1);//Dependant City

                                    oClsRxH_271Master.DState = oSegment.get_DataElementValue(2);//Dependant State

                                    oClsRxH_271Master.DZip = oSegment.get_DataElementValue(3);//Dependant Zip
                                }
                                else if (sSegmentID == "PER")
                                {

                                }
                                #region "REF"

                                else if (sSegmentID == "REF")
                                {
                                    Qlfr = oSegment.get_DataElementValue(1);
                                    switch (Qlfr)
                                    {

                                        case "A6":
                                            //sEmployeeId = oSegment.get_DataElementValue(2);
                                            //sValue.Append("Employee ID : " + sEmployeeId + Environment.NewLine);
                                            oClsRxH_271Master.EmployeeId = oSegment.get_DataElementValue(2);
                                            break;
                                        case "18":
                                            //sPlanNumber = oSegment.get_DataElementValue(2);
                                            //sValue.Append("Plan Number : " + sPlanNumber + Environment.NewLine);

                                            oClsRxH_271Master.HealthPlanNumber = oSegment.get_DataElementValue(2);
                                            oClsRxH_271Master.HealthPlanName = oSegment.get_DataElementValue(3);//health plan name in the REF03 
                                            break;
                                        case "1W":
                                            //sMemberID = oSegment.get_DataElementValue(2);
                                            //sValue.Append("Member ID : " + sMemberID + Environment.NewLine);

                                            oClsRxH_271Master.CardHolderId = oSegment.get_DataElementValue(2);//cardholder ID in the REF02 

                                            oClsRxH_271Master.CardHolderName = oSegment.get_DataElementValue(3);//and cardholder name in the REF03 will be used to populate drug history request and new prescription transactions.
                                            break;
                                        case "6P":
                                            oClsRxH_271Master.GroupId = oSegment.get_DataElementValue(2);//group ID in the REF02 will be used to populate drug history request and new prescription transactions.
                                            oClsRxH_271Master.GroupName = oSegment.get_DataElementValue(3);//group ID in the REF02 will be used to populate drug history request and new prescription transactions.
                                            break;
                                        case "IF":
                                            oClsRxH_271Master.FormularyListId = oSegment.get_DataElementValue(2);//++++++++++++++++++++
                                            oClsRxH_271Master.AlternativeListId = oSegment.get_DataElementValue(3);//++++++++++++++++++++
                                            break;
                                        case "1L":
                                            oClsRxH_271Master.CoverageId = oSegment.get_DataElementValue(3);//++++++++++++++++++++
                                            break;

                                        case "HJ"://ANSI 5010
                                            //HJ        Identity Card Number (* Cardholder ID)
                                            //Strongly recommended by Surescripts.
                                            oClsRxH_271Master.CardHolderId = oSegment.get_DataElementValue(2);//cardholder ID in the REF02 
                                            oClsRxH_271Master.CardHolderName = oSegment.get_DataElementValue(3);
                                            break;
                                        case "49"://ANSI 5010
                                            //49        Family Unit Member (Person Code)
                                            oClsRxH_271Master.PersonCode = oSegment.get_DataElementValue(2);
                                            break;
                                        case "SY"://ANSI 5010
                                            //SY            Social Security Number
                                            //The social security number may not be used for any Federally administered programs such as Medicare.
                                            oClsRxH_271Master.SocialSecurityNumber = oSegment.get_DataElementValue(2);

                                            break;
                                        case "EJ"://ANSI 5010
                                            oClsRxH_271Master.PatientAccountNumber = oSegment.get_DataElementValue(2);
                                            break;
                                        case "ALS":// Alternate List ID
                                            oClsRxH_271Master.AlternativeListId = oSegment.get_DataElementValue(2);
                                            break;
                                        case "CLI"://Coverage List ID
                                            oClsRxH_271Master.CoverageId = oSegment.get_DataElementValue(2);
                                            break;
                                        case "FO"://Drug Formulary Number ID
                                            oClsRxH_271Master.FormularyListId = oSegment.get_DataElementValue(2);
                                            break;
                                        case "IG"://Insurance Policy Number (*Copay ID)
                                            oClsRxH_271Master.CopayId = oSegment.get_DataElementValue(2);
                                            break;
                                        case "N6"://Plan Network ID (*BIN/PCN)
                                            string BinNumber = oSegment.get_DataElementValue(2);
                                            oClsRxH_271Master.BINNumber = oSegment.get_DataElementValue(2);
                                            string sPCNNumber = "";
                                            sPCNNumber = oSegment.get_DataElementValue(3);//need to add property procedure to read the PCN number and to user we will show as BIN/PCN = 234876/AV [David Cross]
                                            if (sPCNNumber != "")
                                            {
                                                if (BinNumber != "")
                                                {
                                                    oClsRxH_271Master.BINNumber = BinNumber + "/" + sPCNNumber;
                                                }
                                                else
                                                {
                                                    oClsRxH_271Master.BINNumber = sPCNNumber;
                                                }
                                            }
                                            else
                                            {
                                                if (BinNumber != "")
                                                {
                                                    oClsRxH_271Master.BINNumber = BinNumber;
                                                }
                                            }
                                            break;

                                    }
                                    #region "Commented  from ANSI 5010"
                                    ////returned Employee ID
                                    //if (Qlfr == "A6")
                                    //{
                                    //    //sEmployeeId = oSegment.get_DataElementValue(2);
                                    //    //sValue.Append("Employee ID : " + sEmployeeId + Environment.NewLine);
                                    //    oClsRxH_271Master.EmployeeId = oSegment.get_DataElementValue(2);
                                    //}
                                    ////If returned, health plan name in the REF03 must be displayed in the end user application per RxHub application guideline 3.6.
                                    //if (Qlfr == "18")
                                    //{
                                    //    //sPlanNumber = oSegment.get_DataElementValue(2);
                                    //    //sValue.Append("Plan Number : " + sPlanNumber + Environment.NewLine);
                                    //    //txtHealthPlanName.Text = oSegment.get_DataElementValue(3);//++++++++++++++++++++
                                    //    oClsRxH_271Master.HealthPlanNumber = oSegment.get_DataElementValue(2);
                                    //    oClsRxH_271Master.HealthPlanName = oSegment.get_DataElementValue(3);//++++++++++++++++++++

                                    //}


                                    ////If returned, cardholder ID in the REF02 and cardholder name in the REF03 will be used to populate drug history request and new prescription transactions.
                                    //else if (Qlfr == "1W")
                                    //{
                                    //    //sMemberID = oSegment.get_DataElementValue(2);
                                    //    //sValue.Append("Member ID : " + sMemberID + Environment.NewLine);
                                    //    //txtCardHolderId.Text = oSegment.get_DataElementValue(2);//++++++++++++++++++++
                                    //    oClsRxH_271Master.CardHolderId = oSegment.get_DataElementValue(2);//++++++++++++++++++++
                                    //    //txtCardHolderName.Text = oSegment.get_DataElementValue(2);//++++++++++++++++++++
                                    //    oClsRxH_271Master.CardHolderName = oSegment.get_DataElementValue(3);//++++++++++++++++++++

                                    //}
                                    ////If returned, group ID in the REF02 will be used to populate drug history request and new prescription transactions.
                                    //else if (Qlfr == "6P")
                                    //{

                                    //    //sGroupNumber = oSegment.get_DataElementValue(2);
                                    //    //sValue.Append("Group Number : " + sGroupNumber + Environment.NewLine);
                                    //    //txtGroupId.Text = oSegment.get_DataElementValue(2);//++++++++++++++++++++
                                    //    oClsRxH_271Master.GroupId = oSegment.get_DataElementValue(2);//group ID in the REF02 will be used to populate drug history request and new prescription transactions.
                                    //    oClsRxH_271Master.GroupName = oSegment.get_DataElementValue(3);//group ID in the REF02 will be used to populate drug history request and new prescription transactions.
                                    //}
                                    ////If returned, formulary list ID in the REF02 and alternative list ID in the REF03 will be used to link to patient formulary and benefit data.
                                    //else if (Qlfr == "IF")
                                    //{
                                    //    //sFormularlyID = oSegment.get_DataElementValue(2);
                                    //    //sValue.Append("Formulary ID : " + sFormularlyID + Environment.NewLine);
                                    //    //sAlternativeID = oSegment.get_DataElementValue(3);
                                    //    //sValue.Append("Alternative ID : " + sAlternativeID + Environment.NewLine);

                                    //    oClsRxH_271Master.FormularyListId = oSegment.get_DataElementValue(2);//++++++++++++++++++++

                                    //    oClsRxH_271Master.AlternativeListId = oSegment.get_DataElementValue(3);//++++++++++++++++++++

                                    //}
                                    ////If returned, coverage ID in the REF03 will be used to link to patient formulary and benefit data.
                                    //else if (Qlfr == "1L")
                                    //{
                                    //    //sFormularlyID = oSegment.get_DataElementValue(2);
                                    //    //sValue.Append("Formulary ID : " + sFormularlyID + Environment.NewLine);

                                    //    oClsRxH_271Master.CoverageId = oSegment.get_DataElementValue(3);//++++++++++++++++++++

                                    //}
                                    ////If returned, BIN number in the REF02 will be used to populate new prescription transactions.
                                    //else if (Qlfr == "N6")
                                    //{
                                    //    //sBIN = oSegment.get_DataElementValue(2);
                                    //    //sValue.Append("BIN : " + sBIN + Environment.NewLine);
                                    //    //sPCN = oSegment.get_DataElementValue(3);
                                    //    //sValue.Append("PCN : " + sPCN + Environment.NewLine);

                                    //    oClsRxH_271Master.BINNumber = oSegment.get_DataElementValue(2);//++++++++++++++++++++

                                    //}
                                    ////If returned, copay ID in the REF03 will be used to link to patient formulary and benefit data.
                                    //else if (Qlfr == "IG")
                                    //{
                                    //    //sBIN = oSegment.get_DataElementValue(2);
                                    //    //sValue.Append("BIN : " + sBIN + Environment.NewLine);
                                    //    //sPCN = oSegment.get_DataElementValue(3);
                                    //    //sValue.Append("PCN : " + sPCN + Environment.NewLine);

                                    //    oClsRxH_271Master.CopayId = oSegment.get_DataElementValue(3);//++++++++++++++++++++

                                    //}

                                    #endregion "Commented  from ANSI 5010"

                                }
                                #endregion "REF"
                                else if (sSegmentID == "AAA")
                                {
                                    if (oSegment.get_DataElementValue(1) == "N")
                                    {
                                        //sValue.Append(oSegment.get_DataElementValue(2) + Environment.NewLine);
                                        if (oSegment.get_DataElementValue(3).Trim() != "")
                                        {
                                            //listResponse.Items.Add("Payer Rejection Reason: " + GetSubscriberRejectionReason(oSegment.get_DataElementValue(3)));
                                            //listResponse.Items.Add("Payer Follow up: " + GetSubscriberFollowUp(oSegment.get_DataElementValue(4)));
                                        }

                                        //EDIReturnResult = oSegment.get_DataElementValue(3).Trim() + "-" + oSegment.get_DataElementValue(4).Trim();

                                    }
                                }
                                #region "Dependant DMG"

                                else if (sSegmentID == "DMG")
                                {

                                    oClsRxH_271Master.DDOB = oSegment.get_DataElementValue(2);//Dependant Date of birth

                                    oClsRxH_271Master.DGender = oSegment.get_DataElementValue(3);//Dependant Gender
                                }
                                #endregion "Depandant DMG"

                                #region "Dependant INS"
                                else if (sSegmentID == "INS")
                                {
                                    //If INS03 is populated with �001� and INS04 is populated with �25�, some patient demographic information returned in the 271 response differs from what was submitted in the 270 request
                                    if (oSegment.get_DataElementValue(3) == "001" && oSegment.get_DataElementValue(4) == "25")
                                    {

                                        oClsRxH_271Master.IsDependentdemoChange = "True";
                                        //%%%%%%%%%%%%%%%%%%%%%%%%PATIENT MATCH VERIFICATION

                                        //*********since in oPatient.DOB we have binded the date in {7/5/1975 12:00:00 AM}	format so in 271 file generation we send 19750705(year month day) format, so to match we hv written the following code.
                                        string _PatientDOB = "";
                                        if (oPatient.DOB != null)
                                        {
                                            _PatientDOB = oPatient.DOB.Date.ToString("yyyyMMdd");
                                        }
                                        //*********

                                        //*********since in oPatient.Gender we have binded "Male", "Female", "Other" so in 271 file generation we send "M","F","O" so to match we hv written the following code.
                                        //string _PatientGender = "";
                                        //if (oPatient.Gender == "Male")
                                        //{
                                        //    _PatientGender = "M";
                                        //}
                                        //else if (oPatient.Gender == "Female")
                                        //{
                                        //    _PatientGender = "F";
                                        //}
                                        //else if (oPatient.Gender == "Other")
                                        //{
                                        //    _PatientGender = "O";
                                        //}
                                        //*********
                                        //StringBuilder strmessage = new StringBuilder();
                                        //strmessage.Append("Patient Match Verification : The Response 271 file has different data than that of 270 Request file");
                                        //strmessage.Append(Environment.NewLine);

                                        //StringBuilder str270PatientDemographics = new StringBuilder();
                                        //str270PatientDemographics.Append("270 Request contains : " + oPatient.LastName + " " + oPatient.FirstName + " " + oPatient.MiddleName + " " + oPatient.PatientAddress.Zip + " " + _PatientDOB + " " + _PatientGender);
                                        //str270PatientDemographics.Append(Environment.NewLine);

                                        //strmessage.Append(str270PatientDemographics);

                                        //StringBuilder str271PatientDemographics = new StringBuilder();
                                        //str271PatientDemographics.Append("271 Response contains : " + oClsRxH_271Master.DLastName + " " + oClsRxH_271Master.DFirstName + " " + oClsRxH_271Master.DMiddleName + " " + oClsRxH_271Master.DZip + " " + oClsRxH_271Master.DDOB + " " + oClsRxH_271Master.DGender);
                                        oClsRxH_271Master.DependentdemoChgLastName = oClsRxH_271Master.DLastName;
                                        oClsRxH_271Master.DependentdemochgFirstName = oClsRxH_271Master.DFirstName;
                                        oClsRxH_271Master.DependentdemoChgMiddleName = oClsRxH_271Master.DMiddleName;
                                        oClsRxH_271Master.DependentdemoChgZip = oClsRxH_271Master.DZip;
                                        oClsRxH_271Master.DependentdemoChgDOB = oClsRxH_271Master.DDOB;
                                        oClsRxH_271Master.DependentdemoChgGender = oClsRxH_271Master.DGender;
                                        oClsRxH_271Master.DependentdemoChgState = oClsRxH_271Master.DState;
                                        oClsRxH_271Master.DependentdemoChgCity = oClsRxH_271Master.DCity;
                                        oClsRxH_271Master.DependentdemoChgSSN = oClsRxH_271Master.DSSN;
                                        oClsRxH_271Master.DependentdemoChgAddress1 = oClsRxH_271Master.DAddress1;
                                        oClsRxH_271Master.DependentdemoChgAddress2 = oClsRxH_271Master.DAddress2;

                                        //strmessage.Append(str271PatientDemographics);

                                        //MessageBox.Show(strmessage.ToString(), "gloRxHub", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        //%%%%%%%%%%%%%%%%%%%%%%%%PATIENT MATCH VERIFICATION
                                    }
                                    else
                                    {

                                        oClsRxH_271Master.IsDependentdemoChange = "False";
                                    }

                                    if (oSegment.get_DataElementValue(1) == "Y")
                                    {
                                        if (oSegment.get_DataElementValue(2) == "18")
                                        {
                                            oClsRxH_271Master.RelationshipCode = oSegment.get_DataElementValue(2);
                                            oClsRxH_271Master.RelationshipDescription = "Self";
                                        }

                                    }
                                    else//subscriber is dependant
                                    {
                                        oClsRxH_271Master.RelationshipCode = oSegment.get_DataElementValue(2);
                                        oClsRxH_271Master.RelationshipDescription = "Dependent";
                                    }
                                }
                                #region "DTP"

                                else if (sSegmentID == "DTP")
                                {
                                    Qlfr = oSegment.get_DataElementValue(1);
                                    if (Qlfr == "307")//Eligiblity Range of dates when the subscriber or dependent were eligible for benefits
                                    {
                                        //oClsRxH_271Master.EligiblityDate = oSegment.get_DataElementValue(3);//commented for ANSI 5010
                                    }
                                    else if (Qlfr == "472")//Service (Recommended by RxHub) Begin and end dates of the service being rendered
                                    {
                                        //oClsRxH_271Master.ServiceDate = oSegment.get_DataElementValue(3);//commented for ANSI 5010
                                    }
                                    //sValue.Append("Subscriber Service : " + oSegment.get_DataElementValue(1) + Environment.NewLine);
                                    //sValue.Append("Subscriber Date : " + oSegment.get_DataElementValue(2) + Environment.NewLine);
                                    //sValue.Append("Subscriber Service Date : " + oSegment.get_DataElementValue(3) + Environment.NewLine);
                                }
                                #endregion "DTP"


                                #endregion "Dependant INS"
                            }

                            else if (sLoopSection == "HL;NM1;EB")
                            {
                                #region "Dependant EB"

                                if (sSegmentID == "EB")
                                {
                                    Qlfr = oSegment.get_DataElementValue(1);

                                    //* This free text is used for Specialty Pharmacy and LTC since there is not a service type
                                    //code available to use. The text SPECIALTY PHARMACY will indicate this EB loop is for
                                    //Specialty Phamiacy and the text LTC will indicate this is for Long Term Care.
                                    Qlfr_PharmacyCode = oSegment.get_DataElementValue(3); //Pharmacy for which eligibility coverage is considered



                                    if (blnNM1Found == false)
                                    {
                                        //Only if the Qlfr =1 tht means pharnacy has active coverage for claim then make the IsPharmacy flag to true,
                                        //else keep it false
                                        #region " Subscriber EB Segment (When NM1 is IL) "

                                        if (Qlfr == "1")//Active Coverage
                                        {
                                            //listResponse.Items.Add("Active Coverage: " + oSegment.get_DataElementValue(3).Trim());
                                            if (oSegment.get_DataElementValue(3) == "88")//Retail coverage
                                            {
                                                oClsRxH_271Master.IsRetailPharmacyEligible = "Yes";

                                                #region "Insurance type code - Retail"
                                                Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
                                                if (Qlfr_InsuranceTypeCode != "")
                                                {
                                                    if (Qlfr_InsuranceTypeCode == "47")
                                                    {
                                                        oClsRxH_271Master.RetailInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                    }
                                                    else if (Qlfr_InsuranceTypeCode == "CP")
                                                    {
                                                        oClsRxH_271Master.RetailInsTypeCode = "Medicare Conditionally Primary";
                                                    }
                                                    else if (Qlfr_InsuranceTypeCode == "MC")
                                                    {
                                                        oClsRxH_271Master.RetailInsTypeCode = "Medicaid";
                                                    }
                                                    else if (Qlfr_InsuranceTypeCode == "MP")
                                                    {
                                                        oClsRxH_271Master.RetailInsTypeCode = "Medicare Primary";
                                                    }
                                                    else if (Qlfr_InsuranceTypeCode == "OT")
                                                    {
                                                        oClsRxH_271Master.RetailInsTypeCode = "Other (Used for Medicare Part D)";
                                                    }

                                                }
                                                #endregion "Insurance type code - Retail"

                                                //oClsRxH_271Master.RetailInsTypeCode = oSegment.get_DataElementValue(4);
                                                oClsRxH_271Master.RetailPharmacyCoveragePlanName = oSegment.get_DataElementValue(5);
                                                oClsRxH_271Master.RetailMonetaryAmount = oSegment.get_DataElementValue(7);
                                                oClsRxH_271Master.RetailPharmacyEligiblityorBenefitInfo = "Active Coverage";
                                            //    IsRetailPharmacy = true;

                                            }
                                            if (oSegment.get_DataElementValue(3) == "90")//Mail coverage
                                            {
                                                oClsRxH_271Master.IsMailOrdRxDrugEligible = "Yes";
                                                #region "Insurance type code - Mail"
                                                if (Qlfr_InsuranceTypeCode != "")
                                                {
                                                    switch (Qlfr_InsuranceTypeCode)
                                                    {
                                                        case "47":
                                                            oClsRxH_271Master.RetailInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                            break;
                                                        case "CP":
                                                            oClsRxH_271Master.RetailInsTypeCode = "Medicare Conditionally Primary";
                                                            break;
                                                        case "MC":
                                                            oClsRxH_271Master.RetailInsTypeCode = "Medicaid";
                                                            break;
                                                        case "MP":
                                                            oClsRxH_271Master.RetailInsTypeCode = "Medicare Primary";
                                                            break;
                                                        case "OT":
                                                            oClsRxH_271Master.RetailInsTypeCode = "Other (Used for Medicare Part D)";
                                                            break;


                                                    }

                                                }
                                                #endregion "Insurance type code - Mail"
                                                //oClsRxH_271Master.MailOrderInsTypeCode = oSegment.get_DataElementValue(4);
                                                oClsRxH_271Master.MailOrdRxDrugCoveragePlanName = oSegment.get_DataElementValue(5);
                                                oClsRxH_271Master.MailOrderMonetaryAmount = oSegment.get_DataElementValue(7);
                                                oClsRxH_271Master.MailOrdRxDrugEligiblityorBenefitInfo = "Active Coverage";
                                           //     IsMailOrdRx = true;

                                            }
                                            //Added for ANSI 5010 
                                            if (oSegment.get_DataElementValue(3) == "30")//Health Plan Benefit Coverage
                                            {
                                                #region "Insurance type code"

                                                Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);

                                                if (Qlfr_InsuranceTypeCode != "")
                                                {
                                                    switch (Qlfr_InsuranceTypeCode)
                                                    {
                                                        case "47":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                            break;
                                                        case "CP":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Medicare Conditionally Primary";
                                                            break;
                                                        case "MC":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Medicaid";
                                                            break;
                                                        case "MP":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Medicare Primary";
                                                            break;
                                                        case "OT":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Other (Used for Medicare Part D)";
                                                            break;

                                                    }
                                                }

                                                #endregion "Insurance type code"
                                                oClsRxH_271Master.HealthPlanBenefitEligibilityInfo = "Active Coverage";
                                                oClsRxH_271Master.HealthPlanBenefitCoverageName = oSegment.get_DataElementValue(5);//added for ANSI 5010
                                            }
                                            //Added for ANSI 5010 
                                            // if Empty/Null        Specialty Phamacy or LTC (See MSG segment) pg 133 of "Prescription Benefit IG 2011-04-15.pdf"
                                            if (oSegment.get_DataElementValue(3) == "")
                                            {
                                                #region "Insurance type code"

                                                Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);

                                                if (Qlfr_InsuranceTypeCode != "")
                                                {
                                                    switch (Qlfr_InsuranceTypeCode)
                                                    {
                                                        case "47":
                                                            break;

                                                        case "CP":
                                                            break;

                                                        case "MC":
                                                            break;

                                                        case "MP":
                                                            break;

                                                        case "OT":
                                                            break;

                                                    }
                                                }

                                                #endregion "Insurance type code"
                                                //string sHealthPlanBenefitCoverageName = "";//new property proc need to be added instead of string variable
                                                sSpecialtyorLTCPhrmCovName = oSegment.get_DataElementValue(5);//new property proc need to be added instead of string variable
                                            }
                                        }

                                        else if (Qlfr == "6")//Inactive - if inactive then Retail-99 and Mail-88 loops will not be sent
                                        {
                                            //6         Inactive
                                            //*If the member is inactive, then no other EB loops are required to be sent.

                                            if (oSegment.get_DataElementValue(3) == "30")//Health Plan Benefit Coverage
                                            {
                                                #region "Insurance type code"

                                                Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);

                                                if (Qlfr_InsuranceTypeCode != "")
                                                {
                                                    switch (Qlfr_InsuranceTypeCode)
                                                    {
                                                        case "47":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                            break;
                                                        case "CP":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Medicare Conditionally Primary";
                                                            break;
                                                        case "MC":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Medicaid";
                                                            break;
                                                        case "MP":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Medicare Primary";
                                                            break;
                                                        case "OT":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Other (Used for Medicare Part D)";
                                                            break;

                                                    }
                                                }

                                                #endregion "Insurance type code"

                                                oClsRxH_271Master.IsRetailPharmacyEligible = "NO";
                                                oClsRxH_271Master.RetailMonetaryAmount = oSegment.get_DataElementValue(7);
                                                oClsRxH_271Master.RetailPharmacyEligiblityorBenefitInfo = "Inactive";
                                         //       IsRetailPharmacy = true;

                                                oClsRxH_271Master.IsMailOrdRxDrugEligible = "NO";
                                                oClsRxH_271Master.MailOrdRxDrugEligiblityorBenefitInfo = "Inactive";
                                                oClsRxH_271Master.MailOrderMonetaryAmount = oSegment.get_DataElementValue(7);
                                         //       IsMailOrdRx = true;

                                                oClsRxH_271Master.HealthPlanBenefitEligibilityInfo = "Inactive";
                                                oClsRxH_271Master.HealthPlanBenefitCoverageName = oSegment.get_DataElementValue(5);//added for ANSI 5010
                                            }



                                            #region "commented for ANSI 5010"
                                            //commented for ANSI 5010
                                            //if (oSegment.get_DataElementValue(3).Trim() == "88")
                                            //{
                                            //    oClsRxH_271Master.IsRetailPharmacyEligible = "NO";
                                            //    #region "Insurance type code - Retail"
                                            //    Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
                                            //    if (Qlfr_InsuranceTypeCode != "")
                                            //    {
                                            //        if (Qlfr_InsuranceTypeCode == "47")
                                            //        {
                                            //            oClsRxH_271Master.RetailInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                            //        }
                                            //        else if (Qlfr_InsuranceTypeCode == "CP")
                                            //        {
                                            //            oClsRxH_271Master.RetailInsTypeCode = "Medicare Conditionally Primary";
                                            //        }
                                            //        else if (Qlfr_InsuranceTypeCode == "MC")
                                            //        {
                                            //            oClsRxH_271Master.RetailInsTypeCode = "Medicaid";
                                            //        }
                                            //        else if (Qlfr_InsuranceTypeCode == "MP")
                                            //        {
                                            //            oClsRxH_271Master.RetailInsTypeCode = "Medicare Primary";
                                            //        }
                                            //        else if (Qlfr_InsuranceTypeCode == "OT")
                                            //        {
                                            //            oClsRxH_271Master.RetailInsTypeCode = "Other (Used for Medicare Part D)";
                                            //        }

                                            //    }
                                            //    #endregion "Insurance type code - Retail"
                                            //    //oClsRxH_271Master.RetailInsTypeCode = oSegment.get_DataElementValue(4);
                                            //    //oClsRxH_271Master.PharmacyCoveragePlanName = oSegment.get_DataElementValue(5);
                                            //    oClsRxH_271Master.RetailMonetaryAmount = oSegment.get_DataElementValue(7);
                                            //    oClsRxH_271Master.RetailPharmacyEligiblityorBenefitInfo = "Inactive";
                                            //    IsRetailPharmacy = true;

                                            //}
                                            //if (oSegment.get_DataElementValue(3) == "90")
                                            //{
                                            //    oClsRxH_271Master.IsMailOrdRxDrugEligible = "NO";
                                            //    #region "Insurance type code - Mail"
                                            //    Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
                                            //    if (Qlfr_InsuranceTypeCode != "")
                                            //    {
                                            //        if (Qlfr_InsuranceTypeCode == "47")
                                            //        {
                                            //            oClsRxH_271Master.MailOrderInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                            //        }
                                            //        else if (Qlfr_InsuranceTypeCode == "CP")
                                            //        {
                                            //            oClsRxH_271Master.MailOrderInsTypeCode = "Medicare Conditionally Primary";
                                            //        }
                                            //        else if (Qlfr_InsuranceTypeCode == "MC")
                                            //        {
                                            //            oClsRxH_271Master.MailOrderInsTypeCode = "Medicaid";
                                            //        }
                                            //        else if (Qlfr_InsuranceTypeCode == "MP")
                                            //        {
                                            //            oClsRxH_271Master.MailOrderInsTypeCode = "Medicare Primary";
                                            //        }
                                            //        else if (Qlfr_InsuranceTypeCode == "OT")
                                            //        {
                                            //            oClsRxH_271Master.MailOrderInsTypeCode = "Other (Used for Medicare Part D)";
                                            //        }

                                            //    }
                                            //    #endregion "Insurance type code - Mail"
                                            //    //oClsRxH_271Master.MailOrderInsTypeCode = oSegment.get_DataElementValue(4);
                                            //    // oClsRxH_271Master.MailOrdRxDrugCoveragePlanName = oSegment.get_DataElementValue(5);
                                            //    oClsRxH_271Master.MailOrdRxDrugEligiblityorBenefitInfo = "Inactive";
                                            //    oClsRxH_271Master.MailOrderMonetaryAmount = oSegment.get_DataElementValue(7);
                                            //    IsMailOrdRx = true;

                                            //}
                                            //commented for ANSI 5010
                                            #endregion "commented for ANSI 5010"

                                        }
                                        else if (Qlfr == "G")//Out Of Pocket (Stop Loss)
                                        {
                                            //listResponse.Items.Add("Deductibles: " + oSegment.get_DataElementValue(1).Trim());
                                            if (oSegment.get_DataElementValue(3).Trim() == "88")
                                            {
                                                oClsRxH_271Master.IsRetailPharmacyEligible = "NO";
                                                #region "Insurance type code - Retail"
                                                Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
                                                if (Qlfr_InsuranceTypeCode != "")
                                                {
                                                    if (Qlfr_InsuranceTypeCode == "47")
                                                    {
                                                        oClsRxH_271Master.RetailInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                    }
                                                    else if (Qlfr_InsuranceTypeCode == "CP")
                                                    {
                                                        oClsRxH_271Master.RetailInsTypeCode = "Medicare Conditionally Primary";
                                                    }
                                                    else if (Qlfr_InsuranceTypeCode == "MC")
                                                    {
                                                        oClsRxH_271Master.RetailInsTypeCode = "Medicaid";
                                                    }
                                                    else if (Qlfr_InsuranceTypeCode == "MP")
                                                    {
                                                        oClsRxH_271Master.RetailInsTypeCode = "Medicare Primary";
                                                    }
                                                    else if (Qlfr_InsuranceTypeCode == "OT")
                                                    {
                                                        oClsRxH_271Master.RetailInsTypeCode = "Other (Used for Medicare Part D)";
                                                    }

                                                }
                                                #endregion "Insurance type code - Retail"
                                                oClsRxH_271Master.RetailPharmacyCoveragePlanName = oSegment.get_DataElementValue(5);
                                                oClsRxH_271Master.RetailPharmacyEligiblityorBenefitInfo = "Out of Pocket (Stop Loss)";
                                                oClsRxH_271Master.RetailMonetaryAmount = oSegment.get_DataElementValue(7);
                                          //      IsRetailPharmacy = true;

                                            }
                                            if (oSegment.get_DataElementValue(3) == "90")
                                            {
                                                oClsRxH_271Master.IsMailOrdRxDrugEligible = "NO";
                                                #region "Insurance type code - Mail"
                                                Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
                                                if (Qlfr_InsuranceTypeCode != "")
                                                {
                                                    if (Qlfr_InsuranceTypeCode == "47")
                                                    {
                                                        oClsRxH_271Master.MailOrderInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                    }
                                                    else if (Qlfr_InsuranceTypeCode == "CP")
                                                    {
                                                        oClsRxH_271Master.MailOrderInsTypeCode = "Medicare Conditionally Primary";
                                                    }
                                                    else if (Qlfr_InsuranceTypeCode == "MC")
                                                    {
                                                        oClsRxH_271Master.MailOrderInsTypeCode = "Medicaid";
                                                    }
                                                    else if (Qlfr_InsuranceTypeCode == "MP")
                                                    {
                                                        oClsRxH_271Master.MailOrderInsTypeCode = "Medicare Primary";
                                                    }
                                                    else if (Qlfr_InsuranceTypeCode == "OT")
                                                    {
                                                        oClsRxH_271Master.MailOrderInsTypeCode = "Other (Used for Medicare Part D)";
                                                    }

                                                }
                                                #endregion "Insurance type code - Mail"

                                                oClsRxH_271Master.MailOrdRxDrugCoveragePlanName = oSegment.get_DataElementValue(5);
                                                oClsRxH_271Master.MailOrdRxDrugEligiblityorBenefitInfo = "Out of Pocket (Stop Loss)";
                                                oClsRxH_271Master.MailOrderMonetaryAmount = oSegment.get_DataElementValue(7);
                                              //  IsMailOrdRx = true;

                                            }
                                            if (oSegment.get_DataElementValue(3) == "30")//Health Plan Benefit Coverage
                                            {
                                                #region "Insurance type code"

                                                Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);

                                                if (Qlfr_InsuranceTypeCode != "")
                                                {
                                                    switch (Qlfr_InsuranceTypeCode)
                                                    {
                                                        case "47":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                            break;
                                                        case "CP":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Medicare Conditionally Primary";
                                                            break;
                                                        case "MC":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Medicaid";
                                                            break;
                                                        case "MP":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Medicare Primary";
                                                            break;
                                                        case "OT":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Other (Used for Medicare Part D)";
                                                            break;

                                                    }
                                                }

                                                #endregion "Insurance type code"

                                                oClsRxH_271Master.HealthPlanBenefitEligibilityInfo = "Out of Pocket (Stop Loss)";
                                                oClsRxH_271Master.HealthPlanBenefitCoverageName = oSegment.get_DataElementValue(5);//added for ANSI 5010
                                            }
                                        }
                                        else if (Qlfr == "V")//Cannot Process
                                        {
                                            //listResponse.Items.Add("Deductibles: " + oSegment.get_DataElementValue(1).Trim());
                                            if (oSegment.get_DataElementValue(3).Trim() == "88")
                                            {
                                                oClsRxH_271Master.IsRetailPharmacyEligible = "NO";
                                                #region "Insurance type code - Retail"
                                                Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
                                                if (Qlfr_InsuranceTypeCode != "")
                                                {
                                                    if (Qlfr_InsuranceTypeCode == "47")
                                                    {
                                                        oClsRxH_271Master.RetailInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                    }
                                                    else if (Qlfr_InsuranceTypeCode == "CP")
                                                    {
                                                        oClsRxH_271Master.RetailInsTypeCode = "Medicare Conditionally Primary";
                                                    }
                                                    else if (Qlfr_InsuranceTypeCode == "MC")
                                                    {
                                                        oClsRxH_271Master.RetailInsTypeCode = "Medicaid";
                                                    }
                                                    else if (Qlfr_InsuranceTypeCode == "MP")
                                                    {
                                                        oClsRxH_271Master.RetailInsTypeCode = "Medicare Primary";
                                                    }
                                                    else if (Qlfr_InsuranceTypeCode == "OT")
                                                    {
                                                        oClsRxH_271Master.RetailInsTypeCode = "Other (Used for Medicare Part D)";
                                                    }

                                                }
                                                #endregion "Insurance type code - Retail"

                                                oClsRxH_271Master.RetailPharmacyCoveragePlanName = oSegment.get_DataElementValue(5);
                                                oClsRxH_271Master.RetailPharmacyEligiblityorBenefitInfo = "Cannot Process";
                                                oClsRxH_271Master.RetailMonetaryAmount = oSegment.get_DataElementValue(7);
                                          //      IsRetailPharmacy = true;

                                            }
                                            if (oSegment.get_DataElementValue(3) == "90")
                                            {
                                                oClsRxH_271Master.IsMailOrdRxDrugEligible = "NO";
                                                #region "Insurance type code - Mail"
                                                Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
                                                if (Qlfr_InsuranceTypeCode != "")
                                                {
                                                    if (Qlfr_InsuranceTypeCode == "47")
                                                    {
                                                        oClsRxH_271Master.MailOrderInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                    }
                                                    else if (Qlfr_InsuranceTypeCode == "CP")
                                                    {
                                                        oClsRxH_271Master.MailOrderInsTypeCode = "Medicare Conditionally Primary";
                                                    }
                                                    else if (Qlfr_InsuranceTypeCode == "MC")
                                                    {
                                                        oClsRxH_271Master.MailOrderInsTypeCode = "Medicaid";
                                                    }
                                                    else if (Qlfr_InsuranceTypeCode == "MP")
                                                    {
                                                        oClsRxH_271Master.MailOrderInsTypeCode = "Medicare Primary";
                                                    }
                                                    else if (Qlfr_InsuranceTypeCode == "OT")
                                                    {
                                                        oClsRxH_271Master.MailOrderInsTypeCode = "Other (Used for Medicare Part D)";
                                                    }

                                                }
                                                #endregion "Insurance type code - Mail"

                                                oClsRxH_271Master.MailOrdRxDrugCoveragePlanName = oSegment.get_DataElementValue(5);
                                                oClsRxH_271Master.MailOrdRxDrugEligiblityorBenefitInfo = "Cannot Process";
                                                oClsRxH_271Master.MailOrderMonetaryAmount = oSegment.get_DataElementValue(7);
                                            //    IsMailOrdRx = true;

                                            }
                                            if (oSegment.get_DataElementValue(3) == "30")//Health Plan Benefit Coverage
                                            {
                                                #region "Insurance type code"

                                                Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);

                                                if (Qlfr_InsuranceTypeCode != "")
                                                {
                                                    switch (Qlfr_InsuranceTypeCode)
                                                    {
                                                        case "47":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                            break;
                                                        case "CP":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Medicare Conditionally Primary";
                                                            break;
                                                        case "MC":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Medicaid";
                                                            break;
                                                        case "MP":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Medicare Primary";
                                                            break;
                                                        case "OT":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Other (Used for Medicare Part D)";
                                                            break;

                                                    }
                                                }

                                                #endregion "Insurance type code"

                                                oClsRxH_271Master.HealthPlanBenefitEligibilityInfo = "Cannot Process";
                                                oClsRxH_271Master.HealthPlanBenefitCoverageName = oSegment.get_DataElementValue(5);//added for ANSI 5010
                                            }
                                        }
                                        else if (Qlfr == "I")//Non-Covered, Added for ANSI 5010
                                        {
                                            //listResponse.Items.Add("Deductibles: " + oSegment.get_DataElementValue(1).Trim());
                                            if (oSegment.get_DataElementValue(3).Trim() == "88")
                                            {
                                                oClsRxH_271Master.IsRetailPharmacyEligible = "NO";
                                                #region "Insurance type code - Retail"

                                                Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);

                                                if (Qlfr_InsuranceTypeCode != "")
                                                {
                                                    switch (Qlfr_InsuranceTypeCode)
                                                    {
                                                        case "47":
                                                            oClsRxH_271Master.RetailInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                            break;
                                                        case "CP":
                                                            oClsRxH_271Master.RetailInsTypeCode = "Medicare Conditionally Primary";
                                                            break;
                                                        case "MC":
                                                            oClsRxH_271Master.RetailInsTypeCode = "Medicaid";
                                                            break;
                                                        case "MP":
                                                            oClsRxH_271Master.RetailInsTypeCode = "Medicare Primary";
                                                            break;
                                                        case "OT":
                                                            oClsRxH_271Master.RetailInsTypeCode = "Other (Used for Medicare Part D)";
                                                            break;


                                                    }

                                                }
                                                #endregion "Insurance type code - Retail"
                                                oClsRxH_271Master.RetailPharmacyCoveragePlanName = oSegment.get_DataElementValue(5);
                                                oClsRxH_271Master.RetailPharmacyEligiblityorBenefitInfo = "Non-Covered";
                                                oClsRxH_271Master.RetailMonetaryAmount = oSegment.get_DataElementValue(7);
                                             //   IsRetailPharmacy = true;

                                            }
                                            if (oSegment.get_DataElementValue(3) == "90")
                                            {
                                                oClsRxH_271Master.IsMailOrdRxDrugEligible = "NO";
                                                #region "Insurance type code - Mail"
                                                Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
                                                if (Qlfr_InsuranceTypeCode != "")
                                                {
                                                    if (Qlfr_InsuranceTypeCode == "47")
                                                    {
                                                        oClsRxH_271Master.MailOrderInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                    }
                                                    else if (Qlfr_InsuranceTypeCode == "CP")
                                                    {
                                                        oClsRxH_271Master.MailOrderInsTypeCode = "Medicare Conditionally Primary";
                                                    }
                                                    else if (Qlfr_InsuranceTypeCode == "MC")
                                                    {
                                                        oClsRxH_271Master.MailOrderInsTypeCode = "Medicaid";
                                                    }
                                                    else if (Qlfr_InsuranceTypeCode == "MP")
                                                    {
                                                        oClsRxH_271Master.MailOrderInsTypeCode = "Medicare Primary";
                                                    }
                                                    else if (Qlfr_InsuranceTypeCode == "OT")
                                                    {
                                                        oClsRxH_271Master.MailOrderInsTypeCode = "Other (Used for Medicare Part D)";
                                                    }

                                                }
                                                #endregion "Insurance type code - Mail"
                                                oClsRxH_271Master.MailOrdRxDrugCoveragePlanName = oSegment.get_DataElementValue(5);
                                                oClsRxH_271Master.MailOrdRxDrugEligiblityorBenefitInfo = "Non-Covered";
                                                oClsRxH_271Master.MailOrderMonetaryAmount = oSegment.get_DataElementValue(7);
                                            //    IsMailOrdRx = true;

                                            }
                                            if (oSegment.get_DataElementValue(3) == "30")//Health Plan Benefit Coverage
                                            {
                                                #region "Insurance type code"

                                                Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);

                                                if (Qlfr_InsuranceTypeCode != "")
                                                {
                                                    switch (Qlfr_InsuranceTypeCode)
                                                    {
                                                        case "47":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                            break;
                                                        case "CP":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Medicare Conditionally Primary";
                                                            break;
                                                        case "MC":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Medicaid";
                                                            break;
                                                        case "MP":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Medicare Primary";
                                                            break;
                                                        case "OT":
                                                            oClsRxH_271Master.HlthPlnCovInsTypeCode = "Other (Used for Medicare Part D)";
                                                            break;

                                                    }
                                                }

                                                #endregion "Insurance type code"

                                                oClsRxH_271Master.HealthPlanBenefitEligibilityInfo = "Non-Covered";
                                                oClsRxH_271Master.HealthPlanBenefitCoverageName = oSegment.get_DataElementValue(5);//added for ANSI 5010
                                            }
                                        }


                                        #endregion " Subscriber EB Segment (When NM1 is IL) "
                                    }//IF loop end for Subscriber EB Segment
                                    else
                                    {
                                        if (blnContractedProvider == true)
                                        {
                                            #region " Contracted Provider EB Segment (When NM1 is 13) "
                                            if (Qlfr == "1")//Active Coverage
                                            {
                                                //listResponse.Items.Add("Active Coverage: " + oSegment.get_DataElementValue(3).Trim());
                                                if (oSegment.get_DataElementValue(3) == "88")
                                                {
                                                    oClsRxH_271Master.ContProvRetailsEligible = "Yes";
                                                    oClsRxH_271Master.ContProvRetailCoverageInfo = "Active Coverage";
                                                    //Mail ord insurance type code value
                                                    #region "Insurance type code - Retail"
                                                    Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
                                                    if (Qlfr_InsuranceTypeCode != "")
                                                    {
                                                        if (Qlfr_InsuranceTypeCode == "47")
                                                        {
                                                            oClsRxH_271Master.ContProvRetailInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "CP")
                                                        {
                                                            oClsRxH_271Master.ContProvRetailInsTypeCode = "Medicare Conditionally Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MC")
                                                        {
                                                            oClsRxH_271Master.ContProvRetailInsTypeCode = "Medicaid";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MP")
                                                        {
                                                            oClsRxH_271Master.ContProvRetailInsTypeCode = "Medicare Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "OT")
                                                        {
                                                            oClsRxH_271Master.ContProvRetailInsTypeCode = "Other (Used for Medicare Part D)";
                                                        }

                                                    }
                                                    #endregion "Insurance type code - Retail"
                                                    // oClsRxH_271Master.ContProvRetailInsTypeCode = oSegment.get_DataElementValue(4);
                                                    //Mail ord Monetary amount
                                                    oClsRxH_271Master.ContProvRetailMonetaryAmt = oSegment.get_DataElementValue(7);

                                                }
                                                if (oSegment.get_DataElementValue(3) == "90")
                                                {
                                                    oClsRxH_271Master.ContProvMailOrderEligible = "Yes";
                                                    oClsRxH_271Master.ContProvMailOrderCoverageInfo = "Active Coverage";
                                                    //Mail ord insurance type code value
                                                    #region "Insurance type code - Mail"
                                                    Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
                                                    if (Qlfr_InsuranceTypeCode != "")
                                                    {
                                                        if (Qlfr_InsuranceTypeCode == "47")
                                                        {
                                                            oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "CP")
                                                        {
                                                            oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Medicare Conditionally Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MC")
                                                        {
                                                            oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Medicaid";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MP")
                                                        {
                                                            oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Medicare Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "OT")
                                                        {
                                                            oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Other (Used for Medicare Part D)";
                                                        }

                                                    }
                                                    #endregion "Insurance type code - Mail"
                                                    //oClsRxH_271Master.ContProvMailOrderInsTypeCode = oSegment.get_DataElementValue(4);
                                                    //Mail ord Monetary amount
                                                    oClsRxH_271Master.ContProvMailOrderMonetaryAmt = oSegment.get_DataElementValue(7);
                                                }
                                            }

                                            else if (Qlfr == "6")//Inactive
                                            {
                                                //listResponse.Items.Add("Co-Payment: " + oSegment.get_DataElementValue(1).Trim());
                                                if (oSegment.get_DataElementValue(3).Trim() == "88")
                                                {

                                                    oClsRxH_271Master.ContProvRetailsEligible = "NO";
                                                    oClsRxH_271Master.ContProvRetailCoverageInfo = "Inactive";
                                                    //Mail ord insurance type code value
                                                    #region "Insurance type code - Retail"
                                                    Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
                                                    if (Qlfr_InsuranceTypeCode != "")
                                                    {
                                                        if (Qlfr_InsuranceTypeCode == "47")
                                                        {
                                                            oClsRxH_271Master.ContProvRetailInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "CP")
                                                        {
                                                            oClsRxH_271Master.ContProvRetailInsTypeCode = "Medicare Conditionally Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MC")
                                                        {
                                                            oClsRxH_271Master.ContProvRetailInsTypeCode = "Medicaid";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MP")
                                                        {
                                                            oClsRxH_271Master.ContProvRetailInsTypeCode = "Medicare Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "OT")
                                                        {
                                                            oClsRxH_271Master.ContProvRetailInsTypeCode = "Other (Used for Medicare Part D)";
                                                        }

                                                    }
                                                    #endregion "Insurance type code - Retail"
                                                    //oClsRxH_271Master.ContProvRetailInsTypeCode = oSegment.get_DataElementValue(4);
                                                    //Mail ord Monetary amount
                                                    oClsRxH_271Master.ContProvRetailMonetaryAmt = oSegment.get_DataElementValue(7);
                                                }
                                                if (oSegment.get_DataElementValue(3) == "90")
                                                {
                                                    oClsRxH_271Master.ContProvMailOrderEligible = "NO";
                                                    oClsRxH_271Master.ContProvMailOrderCoverageInfo = "Inactive";
                                                    //Mail ord insurance type code value
                                                    #region "Insurance type code - Mail"
                                                    Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
                                                    if (Qlfr_InsuranceTypeCode != "")
                                                    {
                                                        if (Qlfr_InsuranceTypeCode == "47")
                                                        {
                                                            oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "CP")
                                                        {
                                                            oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Medicare Conditionally Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MC")
                                                        {
                                                            oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Medicaid";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MP")
                                                        {
                                                            oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Medicare Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "OT")
                                                        {
                                                            oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Other (Used for Medicare Part D)";
                                                        }

                                                    }
                                                    #endregion "Insurance type code - Mail"
                                                    //oClsRxH_271Master.ContProvMailOrderInsTypeCode = oSegment.get_DataElementValue(4);
                                                    //Mail ord Monetary amount
                                                    oClsRxH_271Master.ContProvMailOrderMonetaryAmt = oSegment.get_DataElementValue(7);

                                                }
                                            }
                                            else if (Qlfr == "G")//Out Of Pocket (Stop Loss)
                                            {
                                                //listResponse.Items.Add("Deductibles: " + oSegment.get_DataElementValue(1).Trim());
                                                if (oSegment.get_DataElementValue(3).Trim() == "88")
                                                {

                                                    oClsRxH_271Master.ContProvRetailsEligible = "NO";
                                                    oClsRxH_271Master.ContProvRetailCoverageInfo = "Out of Pocket (Stop Loss)";
                                                    //Mail ord insurance type code value
                                                    #region "Insurance type code - Retail"
                                                    Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
                                                    if (Qlfr_InsuranceTypeCode != "")
                                                    {
                                                        if (Qlfr_InsuranceTypeCode == "47")
                                                        {
                                                            oClsRxH_271Master.ContProvRetailInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "CP")
                                                        {
                                                            oClsRxH_271Master.ContProvRetailInsTypeCode = "Medicare Conditionally Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MC")
                                                        {
                                                            oClsRxH_271Master.ContProvRetailInsTypeCode = "Medicaid";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MP")
                                                        {
                                                            oClsRxH_271Master.ContProvRetailInsTypeCode = "Medicare Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "OT")
                                                        {
                                                            oClsRxH_271Master.ContProvRetailInsTypeCode = "Other (Used for Medicare Part D)";
                                                        }

                                                    }
                                                    #endregion "Insurance type code - Retail"
                                                    //oClsRxH_271Master.ContProvRetailInsTypeCode = oSegment.get_DataElementValue(4);
                                                    //Mail ord Monetary amount
                                                    oClsRxH_271Master.ContProvRetailMonetaryAmt = oSegment.get_DataElementValue(7);
                                                }
                                                if (oSegment.get_DataElementValue(3) == "90")
                                                {

                                                    oClsRxH_271Master.ContProvMailOrderEligible = "NO";
                                                    oClsRxH_271Master.ContProvMailOrderCoverageInfo = "Out of Pocket (Stop Loss)";
                                                    //Mail ord insurance type code value
                                                    #region "Insurance type code - Mail"
                                                    Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
                                                    if (Qlfr_InsuranceTypeCode != "")
                                                    {
                                                        if (Qlfr_InsuranceTypeCode == "47")
                                                        {
                                                            oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "CP")
                                                        {
                                                            oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Medicare Conditionally Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MC")
                                                        {
                                                            oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Medicaid";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MP")
                                                        {
                                                            oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Medicare Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "OT")
                                                        {
                                                            oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Other (Used for Medicare Part D)";
                                                        }

                                                    }
                                                    #endregion "Insurance type code - Mail"
                                                    //oClsRxH_271Master.ContProvMailOrderInsTypeCode = oSegment.get_DataElementValue(4);
                                                    //Mail ord Monetary amount
                                                    oClsRxH_271Master.ContProvMailOrderMonetaryAmt = oSegment.get_DataElementValue(7);

                                                }
                                            }
                                            else if (Qlfr == "V")//Cannot Process
                                            {
                                                //listResponse.Items.Add("Deductibles: " + oSegment.get_DataElementValue(1).Trim());
                                                if (oSegment.get_DataElementValue(3).Trim() == "88")
                                                {
                                                    oClsRxH_271Master.ContProvRetailsEligible = "NO";
                                                    oClsRxH_271Master.ContProvRetailCoverageInfo = "Cannot Process";
                                                    //Mail ord insurance type code value
                                                    #region "Insurance type code - Retail"
                                                    Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
                                                    if (Qlfr_InsuranceTypeCode != "")
                                                    {
                                                        if (Qlfr_InsuranceTypeCode == "47")
                                                        {
                                                            oClsRxH_271Master.ContProvRetailInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "CP")
                                                        {
                                                            oClsRxH_271Master.ContProvRetailInsTypeCode = "Medicare Conditionally Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MC")
                                                        {
                                                            oClsRxH_271Master.ContProvRetailInsTypeCode = "Medicaid";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MP")
                                                        {
                                                            oClsRxH_271Master.ContProvRetailInsTypeCode = "Medicare Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "OT")
                                                        {
                                                            oClsRxH_271Master.ContProvRetailInsTypeCode = "Other (Used for Medicare Part D)";
                                                        }

                                                    }
                                                    #endregion "Insurance type code - Retail"
                                                    //oClsRxH_271Master.ContProvRetailInsTypeCode = oSegment.get_DataElementValue(4);
                                                    //Mail ord Monetary amount
                                                    oClsRxH_271Master.ContProvRetailMonetaryAmt = oSegment.get_DataElementValue(7);
                                                }
                                                if (oSegment.get_DataElementValue(3) == "90")
                                                {
                                                    oClsRxH_271Master.ContProvMailOrderEligible = "NO";
                                                    oClsRxH_271Master.ContProvMailOrderCoverageInfo = "Cannot Process";
                                                    //Mail ord insurance type code value
                                                    #region "Insurance type code - Mail"
                                                    Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
                                                    if (Qlfr_InsuranceTypeCode != "")
                                                    {
                                                        if (Qlfr_InsuranceTypeCode == "47")
                                                        {
                                                            oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "CP")
                                                        {
                                                            oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Medicare Conditionally Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MC")
                                                        {
                                                            oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Medicaid";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MP")
                                                        {
                                                            oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Medicare Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "OT")
                                                        {
                                                            oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Other (Used for Medicare Part D)";
                                                        }

                                                    }
                                                    #endregion "Insurance type code - Mail"
                                                    //oClsRxH_271Master.ContProvMailOrderInsTypeCode = oSegment.get_DataElementValue(4);
                                                    //Mail ord Monetary amount
                                                    oClsRxH_271Master.ContProvMailOrderMonetaryAmt = oSegment.get_DataElementValue(7);

                                                }
                                            }
                                            #endregion " Contracted Provider EB Segment (When NM1 is 13) "
                                        }// IF loop end for Contracted provider EB Segment
                                        else if (blnPrimaryPayer == true)
                                        {
                                            #region " Primary Payer EB Segment (When NM1 is PRP) "
                                            if (Qlfr == "1")//Active Coverage
                                            {
                                                //listResponse.Items.Add("Active Coverage: " + oSegment.get_DataElementValue(3).Trim());
                                                if (oSegment.get_DataElementValue(3) == "88")
                                                {

                                                    oClsRxH_271Master.PrimaryPayerRetailsEligible = "Yes";
                                                    oClsRxH_271Master.PrimaryPayerRetailCoverageInfo = "Active Coverage";
                                                    //oClsRxH_271Master.MailOrdRxDrugCoveragePlanName = oSegment.get_DataElementValue(5);
                                                    #region "Insurance type code - Retail"
                                                    Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
                                                    if (Qlfr_InsuranceTypeCode != "")
                                                    {
                                                        if (Qlfr_InsuranceTypeCode == "47")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "CP")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Medicare Conditionally Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MC")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Medicaid";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MP")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Medicare Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "OT")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Other (Used for Medicare Part D)";
                                                        }

                                                    }
                                                    #endregion "Insurance type code - Retail"
                                                    //oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = oSegment.get_DataElementValue(4);
                                                    oClsRxH_271Master.PrimaryPayerRetailMonetaryAmt = oSegment.get_DataElementValue(7);

                                                }
                                                if (oSegment.get_DataElementValue(3) == "90")
                                                {
                                                    oClsRxH_271Master.PrimaryPayerMailOrderEligible = "Yes";
                                                    oClsRxH_271Master.PrimaryPayerMailOrderCoverageInfo = "Active Coverage";
                                                    #region "Insurance type code - Mail"
                                                    Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
                                                    if (Qlfr_InsuranceTypeCode != "")
                                                    {
                                                        if (Qlfr_InsuranceTypeCode == "47")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "CP")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Medicare Conditionally Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MC")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Medicaid";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MP")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Medicare Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "OT")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Other (Used for Medicare Part D)";
                                                        }

                                                    }
                                                    #endregion "Insurance type code - Mail"
                                                    //oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = oSegment.get_DataElementValue(4);
                                                    oClsRxH_271Master.PrimaryPayerMailOrderMonetaryAmt = oSegment.get_DataElementValue(7);

                                                }
                                            }

                                            else if (Qlfr == "6")//Inactive
                                            {
                                                //listResponse.Items.Add("Co-Payment: " + oSegment.get_DataElementValue(1).Trim());
                                                if (oSegment.get_DataElementValue(3).Trim() == "88")
                                                {
                                                    oClsRxH_271Master.PrimaryPayerRetailsEligible = "NO";
                                                    oClsRxH_271Master.PrimaryPayerRetailCoverageInfo = "Inactive";
                                                    //oClsRxH_271Master.MailOrdRxDrugCoveragePlanName = oSegment.get_DataElementValue(5);
                                                    #region "Insurance type code - Retail"
                                                    Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
                                                    if (Qlfr_InsuranceTypeCode != "")
                                                    {
                                                        if (Qlfr_InsuranceTypeCode == "47")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "CP")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Medicare Conditionally Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MC")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Medicaid";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MP")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Medicare Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "OT")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Other (Used for Medicare Part D)";
                                                        }

                                                    }
                                                    #endregion "Insurance type code - Retail"
                                                    //oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = oSegment.get_DataElementValue(4);
                                                    oClsRxH_271Master.PrimaryPayerRetailMonetaryAmt = oSegment.get_DataElementValue(7);

                                                }
                                                if (oSegment.get_DataElementValue(3) == "90")
                                                {
                                                    oClsRxH_271Master.PrimaryPayerMailOrderEligible = "NO";
                                                    oClsRxH_271Master.PrimaryPayerMailOrderCoverageInfo = "Inactive";
                                                    #region "Insurance type code - Mail"
                                                    Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
                                                    if (Qlfr_InsuranceTypeCode != "")
                                                    {
                                                        if (Qlfr_InsuranceTypeCode == "47")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "CP")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Medicare Conditionally Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MC")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Medicaid";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MP")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Medicare Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "OT")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Other (Used for Medicare Part D)";
                                                        }

                                                    }
                                                    #endregion "Insurance type code - Mail"
                                                    oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = oSegment.get_DataElementValue(4);
                                                    oClsRxH_271Master.PrimaryPayerMailOrderMonetaryAmt = oSegment.get_DataElementValue(7);

                                                }
                                            }
                                            else if (Qlfr == "G")//Out Of Pocket (Stop Loss)
                                            {
                                                //listResponse.Items.Add("Deductibles: " + oSegment.get_DataElementValue(1).Trim());
                                                if (oSegment.get_DataElementValue(3).Trim() == "88")
                                                {
                                                    oClsRxH_271Master.PrimaryPayerRetailsEligible = "NO";
                                                    oClsRxH_271Master.PrimaryPayerRetailCoverageInfo = "Out of Pocket (Stop Loss)";
                                                    //oClsRxH_271Master.MailOrdRxDrugCoveragePlanName = oSegment.get_DataElementValue(5);
                                                    #region "Insurance type code - Retail"
                                                    Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
                                                    if (Qlfr_InsuranceTypeCode != "")
                                                    {
                                                        if (Qlfr_InsuranceTypeCode == "47")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "CP")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Medicare Conditionally Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MC")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Medicaid";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MP")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Medicare Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "OT")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Other (Used for Medicare Part D)";
                                                        }

                                                    }
                                                    #endregion "Insurance type code - Retail"
                                                    //oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = oSegment.get_DataElementValue(4);
                                                    oClsRxH_271Master.PrimaryPayerRetailMonetaryAmt = oSegment.get_DataElementValue(7);

                                                }
                                                if (oSegment.get_DataElementValue(3) == "90")
                                                {
                                                    oClsRxH_271Master.PrimaryPayerMailOrderEligible = "NO";
                                                    oClsRxH_271Master.PrimaryPayerMailOrderCoverageInfo = "Out of Pocket (Stop Loss)";
                                                    #region "Insurance type code - Mail"
                                                    Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
                                                    if (Qlfr_InsuranceTypeCode != "")
                                                    {
                                                        if (Qlfr_InsuranceTypeCode == "47")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "CP")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Medicare Conditionally Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MC")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Medicaid";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MP")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Medicare Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "OT")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Other (Used for Medicare Part D)";
                                                        }

                                                    }
                                                    #endregion "Insurance type code - Mail"
                                                    //oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = oSegment.get_DataElementValue(4);
                                                    oClsRxH_271Master.PrimaryPayerMailOrderMonetaryAmt = oSegment.get_DataElementValue(7);

                                                }
                                            }
                                            else if (Qlfr == "V")//Cannot Process
                                            {
                                                //listResponse.Items.Add("Deductibles: " + oSegment.get_DataElementValue(1).Trim());
                                                if (oSegment.get_DataElementValue(3).Trim() == "88")
                                                {
                                                    oClsRxH_271Master.PrimaryPayerRetailsEligible = "NO";
                                                    oClsRxH_271Master.PrimaryPayerRetailCoverageInfo = "Cannot Process";
                                                    //oClsRxH_271Master.MailOrdRxDrugCoveragePlanName = oSegment.get_DataElementValue(5);
                                                    #region "Insurance type code - Retail"
                                                    Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
                                                    if (Qlfr_InsuranceTypeCode != "")
                                                    {
                                                        if (Qlfr_InsuranceTypeCode == "47")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "CP")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Medicare Conditionally Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MC")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Medicaid";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MP")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Medicare Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "OT")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Other (Used for Medicare Part D)";
                                                        }

                                                    }
                                                    #endregion "Insurance type code - Retail"
                                                    //oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = oSegment.get_DataElementValue(4);
                                                    oClsRxH_271Master.PrimaryPayerRetailMonetaryAmt = oSegment.get_DataElementValue(7);

                                                }
                                                if (oSegment.get_DataElementValue(3) == "90")
                                                {
                                                    oClsRxH_271Master.PrimaryPayerMailOrderEligible = "NO";
                                                    oClsRxH_271Master.PrimaryPayerMailOrderCoverageInfo = "Cannot Process";
                                                    #region "Insurance type code - Mail"
                                                    Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
                                                    if (Qlfr_InsuranceTypeCode != "")
                                                    {
                                                        if (Qlfr_InsuranceTypeCode == "47")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "CP")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Medicare Conditionally Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MC")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Medicaid";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "MP")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Medicare Primary";
                                                        }
                                                        else if (Qlfr_InsuranceTypeCode == "OT")
                                                        {
                                                            oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Other (Used for Medicare Part D)";
                                                        }

                                                    }
                                                    #endregion "Insurance type code - Mail"
                                                    //oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = oSegment.get_DataElementValue(4);
                                                    oClsRxH_271Master.PrimaryPayerMailOrderMonetaryAmt = oSegment.get_DataElementValue(7);

                                                }
                                            }
                                            #endregion " PRP EB Segment"
                                        }//IF Loop end for Primary Payer EB Segment


                                    }

                                }
                                #endregion "Dependant EB"

                                #region "Depandant AAA"
                                else if (sSegmentID == "AAA")
                                {
                                    //sValue.Append(oSegment.get_DataElementValue(2) + Environment.NewLine);
                                    if (oSegment.get_DataElementValue(3).Trim() != "")
                                    {
                                        //listResponse.Items.Add("Eligibility Rejection Reason: " + GetSubscriberRejectionReason(oSegment.get_DataElementValue(3)));
                                        //listResponse.Items.Add("Eligibility Follow up: " + GetSubscriberFollowUp(oSegment.get_DataElementValue(4)));
                                    }

                                    //EDIReturnResult = oSegment.get_DataElementValue(3).Trim() + "-" + oSegment.get_DataElementValue(4).Trim();
                                    //AddNote(_PatientId, EDIReturnResult);
                                }
                                #endregion "Depandant AAA"

                                #region "Dependant REF"
                                else if (sSegmentID == "REF")
                                {
                                    Qlfr = oSegment.get_DataElementValue(1);
                                    switch (Qlfr)
                                    {

                                        case "A6":
                                            //sEmployeeId = oSegment.get_DataElementValue(2);
                                            //sValue.Append("Employee ID : " + sEmployeeId + Environment.NewLine);
                                            oClsRxH_271Master.EmployeeId = oSegment.get_DataElementValue(2);
                                            break;
                                        case "18":
                                            //sPlanNumber = oSegment.get_DataElementValue(2);
                                            //sValue.Append("Plan Number : " + sPlanNumber + Environment.NewLine);

                                            oClsRxH_271Master.HealthPlanNumber = oSegment.get_DataElementValue(2);
                                            oClsRxH_271Master.HealthPlanName = oSegment.get_DataElementValue(3);//health plan name in the REF03 
                                            break;
                                        case "1W":
                                            //sMemberID = oSegment.get_DataElementValue(2);
                                            //sValue.Append("Member ID : " + sMemberID + Environment.NewLine);

                                            oClsRxH_271Master.CardHolderId = oSegment.get_DataElementValue(2);//cardholder ID in the REF02 

                                            oClsRxH_271Master.CardHolderName = oSegment.get_DataElementValue(3);//and cardholder name in the REF03 will be used to populate drug history request and new prescription transactions.
                                            break;
                                        case "6P":
                                            oClsRxH_271Master.GroupId = oSegment.get_DataElementValue(2);//group ID in the REF02 will be used to populate drug history request and new prescription transactions.
                                            oClsRxH_271Master.GroupName = oSegment.get_DataElementValue(3);//group ID in the REF02 will be used to populate drug history request and new prescription transactions.
                                            break;
                                        case "IF":
                                            oClsRxH_271Master.FormularyListId = oSegment.get_DataElementValue(2);//++++++++++++++++++++
                                            oClsRxH_271Master.AlternativeListId = oSegment.get_DataElementValue(3);//++++++++++++++++++++
                                            break;
                                        case "1L":
                                            oClsRxH_271Master.CoverageId = oSegment.get_DataElementValue(3);//++++++++++++++++++++
                                            break;

                                        case "HJ"://ANSI 5010
                                            //HJ        Identity Card Number (* Cardholder ID)
                                            //Strongly recommended by Surescripts.
                                            oClsRxH_271Master.CardHolderId = oSegment.get_DataElementValue(2);//cardholder ID in the REF02 
                                            oClsRxH_271Master.CardHolderName = oSegment.get_DataElementValue(3);
                                            break;
                                        case "49"://ANSI 5010
                                            //49        Family Unit Member (Person Code)
                                            oClsRxH_271Master.PersonCode = oSegment.get_DataElementValue(2);
                                            break;
                                        case "SY"://ANSI 5010
                                            //SY            Social Security Number
                                            //The social security number may not be used for any Federally administered programs such as Medicare.
                                            oClsRxH_271Master.SocialSecurityNumber = oSegment.get_DataElementValue(2);

                                            break;
                                        case "EJ"://ANSI 5010
                                            oClsRxH_271Master.PatientAccountNumber = oSegment.get_DataElementValue(2);
                                            break;
                                        case "ALS":// Alternate List ID
                                            oClsRxH_271Master.AlternativeListId = oSegment.get_DataElementValue(2);
                                            break;
                                        case "CLI"://Coverage List ID
                                            oClsRxH_271Master.CoverageId = oSegment.get_DataElementValue(2);
                                            break;
                                        case "FO"://Drug Formulary Number ID
                                            oClsRxH_271Master.FormularyListId = oSegment.get_DataElementValue(2);
                                            break;
                                        case "IG"://Insurance Policy Number (*Copay ID)
                                            oClsRxH_271Master.CopayId = oSegment.get_DataElementValue(2);
                                            break;
                                        case "N6"://Plan Network ID (*BIN/PCN)
                                            string BinNumber = oSegment.get_DataElementValue(2);
                                            oClsRxH_271Master.BINNumber = oSegment.get_DataElementValue(2);
                                            string sPCNNumber = "";
                                            sPCNNumber = oSegment.get_DataElementValue(3);//need to add property procedure to read the PCN number and to user we will show as BIN/PCN = 234876/AV [David Cross]
                                            if (sPCNNumber != "")
                                            {
                                                if (BinNumber != "")
                                                {
                                                    oClsRxH_271Master.BINNumber = BinNumber + "/" + sPCNNumber;
                                                }
                                                else
                                                {
                                                    oClsRxH_271Master.BINNumber = sPCNNumber;
                                                }
                                            }
                                            else
                                            {
                                                if (BinNumber != "")
                                                {
                                                    oClsRxH_271Master.BINNumber = BinNumber;
                                                }
                                            }
                                            break;

                                    }
                                }
                                #endregion "Dependant REF"

                                #region "DTP - Dependant Date"
                                else if (sSegmentID == "DTP")
                                {
                                    //Added for ANSI 5010
                                    #region "Pharmacy Eligibility date/service date according to pharmacy type"

                                    switch (Qlfr_PharmacyCode)
                                    {
                                        case "30"://Health Plan Benefit Coverage 
                                            Qlfr = oSegment.get_DataElementValue(2);
                                            if (Qlfr == "D8")//Eligiblity Range of dates when the subscriber or dependent were eligible for benefits
                                            {
                                                oClsRxH_271Master.HlthPlnBenftCovEligibilityDate = oSegment.get_DataElementValue(3);//commented for ANSI 5010
                                            }
                                            else if (Qlfr == "RD8")//Service (Recommended by RxHub) Begin and end dates of the service being rendered
                                            {
                                                oClsRxH_271Master.HlthPlnBenftCovServiceDate = oSegment.get_DataElementValue(3);//commented for ANSI 5010
                                            }

                                            break;
                                        case "88"://Retail Order Pharmacy
                                            Qlfr = oSegment.get_DataElementValue(2);
                                            if (Qlfr == "D8")//Eligiblity Range of dates when the subscriber or dependent were eligible for benefits
                                            {
                                                oClsRxH_271Master.RetailOrdPhrmEligibilityDate = oSegment.get_DataElementValue(3);//commented for ANSI 5010
                                            }
                                            else if (Qlfr == "RD8")//Service (Recommended by RxHub) Begin and end dates of the service being rendered
                                            {
                                                oClsRxH_271Master.RetailPhrmServiceDate = oSegment.get_DataElementValue(3);//commented for ANSI 5010
                                            }

                                            break;
                                        case "90"://Mail Order Pharmacy
                                            Qlfr = oSegment.get_DataElementValue(2);
                                            if (Qlfr == "D8")//Eligiblity Range of dates when the subscriber or dependent were eligible for benefits
                                            {
                                                oClsRxH_271Master.MailOrdPhrmEligibilityDate = oSegment.get_DataElementValue(3);//commented for ANSI 5010
                                            }
                                            else if (Qlfr == "RD8")//Service (Recommended by RxHub) Begin and end dates of the service being rendered
                                            {
                                                oClsRxH_271Master.MailOrdPhrmServiceDate = oSegment.get_DataElementValue(3);//commented for ANSI 5010
                                            }
                                            break;
                                        case ""://
                                            Qlfr = oSegment.get_DataElementValue(2);
                                            if (Qlfr == "D8")//Eligiblity Range of dates when the subscriber or dependent were eligible for benefits
                                            {
                                                SpecialtyorLTCPhrmEligibilityDate = oSegment.get_DataElementValue(3);//Add new property proc

                                            }
                                            else if (Qlfr == "RD8")//Service (Recommended by RxHub) Begin and end dates of the service being rendered
                                            {
                                                SpecialtyorLTCPhrmServiceDate = oSegment.get_DataElementValue(3);//Add new property proc

                                            }
                                            break;

                                    }
                                    #endregion "Pharmacy Eligibility date/service date according to pharmacy type"
                                    //Commented for Added for ANSI 5010
                                    //Qlfr = oSegment.get_DataElementValue(2);
                                    //if (Qlfr == "D8")//Eligiblity Range of dates when the subscriber or dependent were eligible for benefits
                                    //{
                                    //    //oClsRxH_271Master.EligiblityDate = oSegment.get_DataElementValue(3);//commented for ANSI 5010
                                    //}
                                    //else if (Qlfr == "RD8")//Service (Recommended by RxHub) Begin and end dates of the service being rendered
                                    //{
                                    //    //oClsRxH_271Master.ServiceDate = oSegment.get_DataElementValue(3);//commented for ANSI 5010
                                    //}

                                    //sValue.Append("Subscriber Service : " + oSegment.get_DataElementValue(1) + Environment.NewLine);
                                    //sValue.Append("Subscriber Date : " + oSegment.get_DataElementValue(2) + Environment.NewLine);
                                    //sValue.Append("Subscriber Service Date : " + oSegment.get_DataElementValue(3) + Environment.NewLine);

                                }
                                #endregion "DTP - Dependant Date"
                            }
                            #region "DTP - Dependant Date"
                            else if (sSegmentID == "DTP")
                            {
                                //Added for ANSI 5010
                                #region "Pharmacy Eligibility date/service date according to pharmacy type"

                                switch (Qlfr_PharmacyCode)
                                {
                                    case "30"://Health Plan Benefit Coverage 
                                        Qlfr = oSegment.get_DataElementValue(2);
                                        if (Qlfr == "D8")//Eligiblity Range of dates when the subscriber or dependent were eligible for benefits
                                        {
                                            oClsRxH_271Master.HlthPlnBenftCovEligibilityDate = oSegment.get_DataElementValue(3);//commented for ANSI 5010
                                        }
                                        else if (Qlfr == "RD8")//Service (Recommended by RxHub) Begin and end dates of the service being rendered
                                        {
                                            oClsRxH_271Master.HlthPlnBenftCovServiceDate = oSegment.get_DataElementValue(3);//commented for ANSI 5010
                                        }

                                        break;
                                    case "88"://Retail Order Pharmacy
                                        Qlfr = oSegment.get_DataElementValue(2);
                                        if (Qlfr == "D8")//Eligiblity Range of dates when the subscriber or dependent were eligible for benefits
                                        {
                                            oClsRxH_271Master.RetailOrdPhrmEligibilityDate = oSegment.get_DataElementValue(3);//commented for ANSI 5010
                                        }
                                        else if (Qlfr == "RD8")//Service (Recommended by RxHub) Begin and end dates of the service being rendered
                                        {
                                            oClsRxH_271Master.RetailPhrmServiceDate = oSegment.get_DataElementValue(3);//commented for ANSI 5010
                                        }

                                        break;
                                    case "90"://Mail Order Pharmacy
                                        Qlfr = oSegment.get_DataElementValue(2);
                                        if (Qlfr == "D8")//Eligiblity Range of dates when the subscriber or dependent were eligible for benefits
                                        {
                                            oClsRxH_271Master.MailOrdPhrmEligibilityDate = oSegment.get_DataElementValue(3);//commented for ANSI 5010
                                        }
                                        else if (Qlfr == "RD8")//Service (Recommended by RxHub) Begin and end dates of the service being rendered
                                        {
                                            oClsRxH_271Master.MailOrdPhrmServiceDate = oSegment.get_DataElementValue(3);//commented for ANSI 5010
                                        }
                                        break;
                                    case ""://
                                        Qlfr = oSegment.get_DataElementValue(2);
                                        if (Qlfr == "D8")//Eligiblity Range of dates when the subscriber or dependent were eligible for benefits
                                        {
                                            SpecialtyorLTCPhrmEligibilityDate = oSegment.get_DataElementValue(3);//Add new property proc

                                        }
                                        else if (Qlfr == "RD8")//Service (Recommended by RxHub) Begin and end dates of the service being rendered
                                        {
                                            SpecialtyorLTCPhrmServiceDate = oSegment.get_DataElementValue(3);//Add new property proc

                                        }
                                        break;

                                }
                                #endregion "Pharmacy Eligibility date/service date according to pharmacy type"
                                //Commented for Added for ANSI 5010
                                //Qlfr = oSegment.get_DataElementValue(2);
                                //if (Qlfr == "D8")//Eligiblity Range of dates when the subscriber or dependent were eligible for benefits
                                //{
                                //    //oClsRxH_271Master.EligiblityDate = oSegment.get_DataElementValue(3);//commented for ANSI 5010
                                //}
                                //else if (Qlfr == "RD8")//Service (Recommended by RxHub) Begin and end dates of the service being rendered
                                //{
                                //    //oClsRxH_271Master.ServiceDate = oSegment.get_DataElementValue(3);//commented for ANSI 5010
                                //}

                                //sValue.Append("Subscriber Service : " + oSegment.get_DataElementValue(1) + Environment.NewLine);
                                //sValue.Append("Subscriber Date : " + oSegment.get_DataElementValue(2) + Environment.NewLine);
                                //sValue.Append("Subscriber Service Date : " + oSegment.get_DataElementValue(3) + Environment.NewLine);

                            }
                            #endregion "DTP - Dependant Date"
                            else if (sLoopSection == "HL;NM1;EB;III")
                            {
                                if (sSegmentID == "III")
                                {

                                }
                            }
                            else if (sLoopSection == "HL;NM1;EB;NM1")
                            {
                                if (sSegmentID == "NM1")
                                {
                                    blnNM1Found = true;
                                }
                            }

                        }
                        //***************************
                        #endregion "Dependant Level"
                    }
                    #endregion "Area = 2"
                    
                    ediDataSegment.Set(ref oSegment, (ediDataSegment)oSegment.Next());

                    if (oSegment != null)
                    {
                        oSegmentArray.Add(oSegment);
                        if (oSegment.ID == "SE")
                        {
                            string str = oSegment.get_DataElementValue(2);
                            if (oClsRxH_271Master != null)
                            {
                                oClsRxH_271Master.STLoopCount = str;

                                oClsRxH_271Master.PatientId = _PatientId;
                                oClsRxH_271Master.RxH_271Details.Add(oClsRxH_271Details);

                              //  oClsRxH_271Details.Dispose();
                                oClsRxH_271Details = null;
                                oClsRxH_271Details = new ClsRxH_271Details();

                                oCls271Information.Add(oClsRxH_271Master);
                                SaveEDIResponse271(oClsRxH_271Master);

                                oClsRxH_271Master.Dispose();
                                oClsRxH_271Master = null;
                                oClsRxH_271Master = new ClsRxH_271Master();
                            }
                            blnNM1Found = false;
                        }
                    }

                }


                if (oCls271Information != null)
                {
                    //oClsRxH_271Master.RxH_271Details.Add(oClsRxH_271Details);

                    return oCls271Information;// oClsRxH_271Master;
                }
                else
                {
                    return null;
                }


            }

            catch (Exception ex)
            {
                #region "Disposing objects"
                if (oCls271Information != null)
                {
                    oCls271Information.Dispose();
                    oCls271Information = null;
                }

                if (oClsRxH_271Master != null)
                {
                    oClsRxH_271Master.Dispose();
                    oClsRxH_271Master = null;
                }

                if (oClsRxH_271Details != null)
                {
                    oClsRxH_271Master.Dispose();
                    oClsRxH_271Master = null;
                }
                if (oclsgloRxHubDBLayer != null)
                {
                    oclsgloRxHubDBLayer.Disconnect();
                    oclsgloRxHubDBLayer.Dispose();
                    oclsgloRxHubDBLayer = null;
                }
                //ediDocument dummy = null;
                //SLR: Changed on 4/4/2014
                for (int i = oSegmentArray.Count - 1; i >= 0; i--)
                {
                    ediDataSegment oNewSegment = oSegmentArray[i] as ediDataSegment;
                    oNewSegment.Dispose();
                    oSegmentArray.RemoveAt(i);
                }
                myEDISchemaObject(0, ref dummySchemas);
                myEDIObject(0, ref dummy);
                #endregion "Disposing objects"
              
                gloAuditTrail.gloAuditTrail.ExceptionLog(gloAuditTrail.ActivityModule.Prescription, gloAuditTrail.ActivityCategory.CreatePrescription, gloAuditTrail.ActivityType.General, ex.ToString(), gloAuditTrail.ActivityOutCome.Failure);
                return null;

            }
            finally
            {
                #region "Disposing objects"
                if (oCls271Information != null)
                {
                    oCls271Information.Dispose();
                    oCls271Information = null;
                }

                if (oClsRxH_271Master != null)
                {
                    oClsRxH_271Master.Dispose();
                    oClsRxH_271Master = null;
                }

                if (oClsRxH_271Details != null)
                {
                    oClsRxH_271Details.Dispose();
                    oClsRxH_271Details = null;
                }
                if (oclsgloRxHubDBLayer != null)
                {
                    oclsgloRxHubDBLayer.Disconnect();
                    oclsgloRxHubDBLayer.Dispose();
                    oclsgloRxHubDBLayer = null;
                }
               // FreeEDIObject(ref oEdiDoc);
                //if (oNexSegment != null)
                //{
                //    oNexSegment.Dispose();
                //    oNexSegment=null;
                //}
                //if (oSegment != null)
                //{
                //    oSegment.Dispose();
                //    oSegment = null;

                //}
               // oSegmentArray.Reverse();
                //SLR: Changed on 4/4/2014
                for (int i=oSegmentArray.Count-1; i >= 0;i-- )
                {
                    ediDataSegment oNewSegment = oSegmentArray[i] as ediDataSegment;
                    oNewSegment.Dispose();
                    oSegmentArray.RemoveAt(i);
                }
                myEDISchemaObject(0, ref dummySchemas);
                myEDIObject(0, ref dummy);
                #endregion "Disposing objects"


            }
        }

        private string getRejecttionDescription(string RejectionCode, string LoopName, string SegmentName)
        {
            string _sRejectionDescription = "";
            try
            {
                #region "AAA Information Source level Request Validation (HL)"

                if (LoopName == "Reciever" && SegmentName == "HL")
                {
                    switch (RejectionCode)
                    {
                        case "04":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Authorized Quantity Exceeded";
                            break;
                        case "41":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Authorization/Access Restrictions";
                            break;
                        case "42":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Unable to Respond at Current Time";
                            break;
                        case "79":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Invalid Participant Identification";
                            break;
                        case "80":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": No Response received � Transaction Terminated";
                            break;
                        case "T4":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Payer Name or Identifier Missing";
                            break;

                    }
                }
                #endregion "AAA Information Source level Request Validation (HL)"

                #region "AAA Information Receiver Request Validation (NM1)"

                else if (LoopName == "Reciever" && SegmentName == "NM1")
                {
                    switch (RejectionCode)
                    {
                        case "15":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Required application data missing";
                            break;
                        case "41":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Authorization/Access Restrictions";
                            break;
                        case "43":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Invalid/Missing Provider Identification (Surescripts recommends this for NPI error.)";
                            break;
                        case "44":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Invalid/Missing Provider Name";
                            break;
                        case "45":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Invalid/Missing Provider Specialty";
                            break;
                        case "46":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Invalid/Missing Provider Phone Number";
                            break;
                        case "47":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Invalid/Missing Provider State";
                            break;
                        case "48":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Invalid/Missing Referring Provider Identification Number";
                            break;
                        case "50":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Provider Ineligible for Inquiries";
                            break;
                        case "51":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Provider Not on File";
                            break;
                        case "79":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Invalid Participant Identification";
                            break;
                        case "97":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Invalid or Missing Provider Address";
                            break;
                        case "T4":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Payer Name or Identifier Missing";
                            break;

                    }
                }
                #endregion "AAA Information Receiver Request Validation (NM1)"

                #region "AAA Subscriber Request Validation (NM1)"

                else if (LoopName == "Subscriber" && SegmentName == "NM1")
                {
                    switch (RejectionCode)
                    {
                        case "15":
                            //*At Surescripts � Not enough infomiation for Surescripts to identify patient.
                            //*At PBM � PBM wants more info than what was supplied.
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Required application data missing";
                            break;
                        case "35":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Out of Network";
                            break;
                        case "42":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Unable to Respond at Current Time";
                            break;
                        case "43":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Invalid/Missing Provider Identification";
                            break;
                        case "45":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Invalid/Missing Provider Specialty";
                            break;
                        case "47":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Invalid/Missing Provider State";
                            break;
                        case "48":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Invalid/Missing Referring Provider Identification Number";
                            break;
                        case "49":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Provider is Not Primary Care Physician";
                            break;
                        case "51":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Provider Not on File";
                            break;
                        case "52":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Service Dates Not Within Provider Plan Enrollment";
                            break;
                        case "56":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Inappropriate Date";
                            break;
                        case "57":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Invalid/Missing Date(s) of Service";
                            break;
                        case "58":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Invalid/Missing Date-of-Birth";
                            break;
                        case "60":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Date of Birth Follows Date(s) of Service";
                            break;
                        case "61":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Date of Death Precedes Date(s) of Service";
                            break;
                        case "62":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Date of Service Not Within Allowable Inquiry Period";
                            break;
                        case "63":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Date of Service in Future";
                            break;
                        case "71":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Patient Birth Date Does Not Match That for the Patient on the Database";
                            break;
                        case "72":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Invalid/Missing Subscriber/Insured ID";
                            break;
                        case "73":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Invalid/Missing Subscriber/Insured Name";
                            break;
                        case "74":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Invalid/Missing Subscriber/Insured Gender Code";
                            break;
                        case "75":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Subscriber/Insured Not Found";
                            break;
                        case "76":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Duplicate Subscriber/Insured ID Number";
                            break;
                        case "78":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Subscriber/Insured Not in Group/Plan Identified";
                            break;

                    }
                }
                #endregion "AAA Subscriber Request Validation (NM1)"

                #region "AAA Subscriber Request Validation (EB)"
                else if (LoopName == "Subscriber" && SegmentName == "EB")
                {
                    switch (RejectionCode)
                    {
                        case "15":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Required application data missing";
                            break;
                        case "33":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Input Errors";
                            break;
                        case "52":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Service Dates Not Within Provider Plan Enrollment";
                            break;
                        case "54":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Inappropriate Product/Service ID Qualifier";
                            break;
                        case "55":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Inappropriate Product/Service ID";
                            break;
                        case "56":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Inappropriate Date";
                            break;
                        case "57":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Invalid/Missing Date(s) of Service";
                            break;
                        case "60":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Date of Birth Follows Date(s) of Service";
                            break;
                        case "61":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Date of Death Precedes Date(s) of Service";
                            break;
                        case "62":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Date of Service Not Within Allowable Inquiry Period";
                            break;
                        case "63":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Date of Service in Future";
                            break;
                        case "69":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Inconsistent with Patient�s Age";
                            break;
                        case "70":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Inconsistent with Patient�s Gender";
                            break;
                    }
                }
                #endregion "AAA Subscriber Request Validation (EB)"

                #region "All combined"
                else if (LoopName == "" && SegmentName == "")
                {
                    switch (RejectionCode)
                    {
                        //"AAA Information Source level Request Validation (HL)"
                        case "04":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Authorized Quantity Exceeded";
                            break;
                        case "41":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Authorization/Access Restrictions";
                            break;
                        case "42":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Unable to Respond at Current Time";
                            break;
                        case "79":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Invalid Participant Identification";
                            break;
                        case "80":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": No Response received � Transaction Terminated";
                            break;
                        case "T4":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Payer Name or Identifier Missing";
                            break;

                        //"AAA Information Receiver Request Validation (NM1)"
                        case "15":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Required application data missing";
                            break;
                        //case "41":
                        //    _sRejectionDescription = "Authorization/Access Restrictions";
                        //    break;
                        case "43":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Invalid/Missing Provider Identification (Surescripts recommends this for NPI error.)";
                            break;
                        case "44":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Invalid/Missing Provider Name";
                            break;
                        case "45":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Invalid/Missing Provider Specialty";
                            break;
                        case "46":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Invalid/Missing Provider Phone Number";
                            break;
                        case "47":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Invalid/Missing Provider State";
                            break;
                        case "48":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Invalid/Missing Referring Provider Identification Number";
                            break;
                        case "50":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Provider Ineligible for Inquiries";
                            break;
                        case "51":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Provider Not on File";
                            break;
                        //case "79":
                        //    _sRejectionDescription = "Invalid Participant Identification";
                        //    break;
                        case "97":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Invalid or Missing Provider Address";
                            break;
                        //case "T4":
                        //    _sRejectionDescription = "Payer Name or Identifier Missing";
                        //    break;

                        // #endregion "AAA Subscriber Request Validation (NM1)"
                        //case "15":
                        //    //*At Surescripts � Not enough infomiation for Surescripts to identify patient.
                        //    //*At PBM � PBM wants more info than what was supplied.
                        //    _sRejectionDescription = "Required application data missing";
                        //    break;
                        case "35":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Out of Network";
                            break;
                        //case "42":
                        //    _sRejectionDescription = "Unable to Respond at Current Time";
                        //    break;
                        //case "43":
                        //    _sRejectionDescription = "Invalid/Missing Provider Identification";
                        //    break;
                        //case "45":
                        //    _sRejectionDescription = "Invalid/Missing Provider Specialty";
                        //    break;
                        //case "47":
                        //    _sRejectionDescription = "Invalid/Missing Provider State";
                        //    break;
                        //case "48":
                        //    _sRejectionDescription = "Invalid/Missing Referring Provider Identification Number";
                        //    break;
                        case "49":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Provider is Not Primary Care Physician";
                            break;
                        //case "51":
                        //    _sRejectionDescription = "Provider Not on File";
                        //    break;
                        case "52":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Service Dates Not Within Provider Plan Enrollment";
                            break;
                        case "56":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Inappropriate Date";
                            break;
                        case "57":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Invalid/Missing Date(s) of Service";
                            break;
                        case "58":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Invalid/Missing Date-of-Birth";
                            break;
                        case "60":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Date of Birth Follows Date(s) of Service";
                            break;
                        case "61":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Date of Death Precedes Date(s) of Service";
                            break;
                        case "62":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Date of Service Not Within Allowable Inquiry Period";
                            break;
                        case "63":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Date of Service in Future";
                            break;
                        case "71":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Patient Birth Date Does Not Match That for the Patient on the Database";
                            break;
                        case "72":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Invalid/Missing Subscriber/Insured ID";
                            break;
                        case "73":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Invalid/Missing Subscriber/Insured Name";
                            break;
                        case "74":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Invalid/Missing Subscriber/Insured Gender Code";
                            break;
                        case "75":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Subscriber/Insured Not Found";
                            break;
                        case "76":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Duplicate Subscriber/Insured ID Number";
                            break;
                        case "78":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Subscriber/Insured Not in Group/Plan Identified";
                            break;

                        //"AAA Subscriber Request Validation (EB)"
                        //case "15":
                        //    _sRejectionDescription = "Required application data missing";
                        //    break;
                        case "33":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Input Errors";
                            break;
                        //case "52":
                        //    _sRejectionDescription = "Service Dates Not Within Provider Plan Enrollment";
                        //    break;
                        case "54":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Inappropriate Product/Service ID Qualifier";
                            break;
                        case "55":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Inappropriate Product/Service ID";
                            break;
                        //case "56":
                        //    _sRejectionDescription = "Inappropriate Date";
                        //    break;
                        //case "57":
                        //    _sRejectionDescription = "Invalid/Missing Date(s) of Service";
                        //    break;
                        //case "60":
                        //    _sRejectionDescription = "Date of Birth Follows Date(s) of Service";
                        //    break;
                        //case "61":
                        //    _sRejectionDescription = "Date of Death Precedes Date(s) of Service";
                        //    break;
                        //case "62":
                        //    _sRejectionDescription = "Date of Service Not Within Allowable Inquiry Period";
                        //    break;
                        //case "63":
                        //    _sRejectionDescription = "Date of Service in Future";
                        //break;
                        case "69":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Inconsistent with Patient�s Age";
                            break;
                        case "70":
                            _sRejectionDescription = "Error Code AAA" + RejectionCode + ": Inconsistent with Patient�s Gender";
                            break;
                    }
                }

                #endregion "All Combined"

                return _sRejectionDescription;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "gloEMR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return "";
            }
            finally
            {

            }
        }

        #endregion "ANSI 5010 Functions"

        #region " Procedures And Functions "

        public bool  ProcessRxEligibilityUsingWebService(string nRxModulePatientID , string EDIWebserviceConnectionStr,string EDIWebserviceURL)
        {
            gloWebEDI.gloWebEDI837 myservice=null;
            string strMessage = "";
            bool retVal = false;
            try
            {
                myservice = new gloWebEDI.gloWebEDI837();
                myservice.Url = EDIWebserviceURL;/////this is passed from the settings table
                retVal = myservice.GetRxEligibility(nRxModulePatientID, EDIWebserviceConnectionStr, out strMessage);
                if (retVal == true)
                {
                    return true;
                }
                else
                {
                    if (strMessage != "")
                    {
                        string CustomMsg = "There was a problem while processing RxEligibility" + Environment.NewLine;
                        //MessageBox.Show(CustomMsg + strMessage.ToString(), "gloEMR", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    return false;
                }

            }
            catch (Exception ex)
            {
                gloAuditTrail.gloAuditTrail.ExceptionLog(ex.ToString(), false);
                return false;
            }
            //SLR: Finally dispose myservice
            finally
            {
                if (myservice != null)
                {
                    myservice.Dispose();
                    myservice = null;
                }
            }
        }

        //public  bool Generate270EDI(ClsPatient objPatient)
        //{
        //    string BHT_TransactionRef="";
        //    string strControlNo = "";
        //    bool IsoSubscriberReferenced = false;
        //    try
        //    {
        //        oPatient = objPatient;

        //        //string sEntity = "";
        //        //string sInstance = "";
        //        //SLR: chck ..
        //        oEdiDoc.New();
        //        oEdiDoc.CursorType = DocumentCursorTypeConstants.Cursor_ForwardWrite;
        //        oEdiDoc.set_Property(DocumentPropertyIDConstants.Property_DocumentBufferIO, 2000);

        //        oEdiDoc.SegmentTerminator = "~\r\n";
        //        oEdiDoc.ElementTerminator = "*";
        //        oEdiDoc.CompositeTerminator = ":";

        //        #region " Interchange Segment "
        //        //Create the interchange segment
        //        ediInterchange.Set(ref oInterchange, (ediInterchange)oEdiDoc.CreateInterchange("X", "004010"));
        //        ediDataSegment.Set(ref oSegment, (ediDataSegment)oInterchange.GetDataSegmentHeader());

        //        oSegment.set_DataElementValue(1, 0, "00");
        //        oSegment.set_DataElementValue(3, 0, "01");
        //        //From POC/PPMS this is the Password assigned by RxHub for POC/PPMS
        //        //From RxHub, this is the password for RxHub to get to the POC/PPMS 
        //        oSegment.set_DataElementValue(4, 0, "FXTXGJVZ0W");
        //        oSegment.set_DataElementValue(5, 0, "ZZ");
        //        //From POC/PPMS this is the POC/PPMS participant ID as assigned by RxHub
        //        //From RxHub, this is the RxHub's participant ID
        //        oSegment.set_DataElementValue(6, 0, "T00000000020315");//
        //        oSegment.set_DataElementValue(7, 0, "ZZ");
        //        //From POC/PPMS this is the RxHub's participant ID as assigned by RxHub
        //        //From RxHub, this is the PBM's participant ID
        //        oSegment.set_DataElementValue(8, 0, "RXHUB");
        //        string ISA_Date = Convert.ToString(gloDateMaster.gloDate.DateAsNumber(DateTime.Now.ToShortDateString()));
        //        oSegment.set_DataElementValue(9, 0, gloDateMaster.gloDate.DateAsNumber(DateTime.Now.ToShortDateString()).ToString().Substring(2));//txtEnquiryDate.Text.Trim());//"010821");
        //        string ISA_Time = Convert.ToString(gloDateMaster.gloTime.TimeAsNumber(DateTime.Now.ToLocalTime().ToShortTimeString()));
        //        oSegment.set_DataElementValue(10, 0, FormattedTime(ISA_Time).Trim());
        //        oSegment.set_DataElementValue(11, 0, "U");
        //        oSegment.set_DataElementValue(12, 0, "00401");
        //        //Create a unique control number as per the doc. it is 9 digit
        //        //DateTime.Now.Date.ToString("MMddyyyy") + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + DateTime.Now.Millisecond.ToString(); //Convert.ToString(gloDateMaster.gloDate.DateAsNumber(DateTime.Now.ToShortDateString())) + Convert.ToString(gloDateMaster.gloTime.TimeAsNumber(DateTime.Now.ToLocalTime().ToShortTimeString())).Trim();
        //        //for generating the unique control no we hv take Milliseconds/Seconds/Minutes/and(hour + 100) so tht we can generate 9 digit unique value.
        //        strControlNo = DateTime.Now.Millisecond.ToString("#000") + DateTime.Now.Second.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Hour.ToString(); //Convert.ToString(gloDateMaster.gloDate.DateAsNumber(DateTime.Now.ToShortDateString())) + Convert.ToString(gloDateMaster.gloTime.TimeAsNumber(DateTime.Now.ToLocalTime().ToShortTimeString())).Trim();
        //        oSegment.set_DataElementValue(13, 0, strControlNo);//"736422078"
        //        oSegment.set_DataElementValue(14, 0, "1");
        //        oSegment.set_DataElementValue(15, 0, "T");
        //        oSegment.set_DataElementValue(16, 0, ":");

        //        #endregion " Interchange Segment "

        //        #region " Functional Group "

        //        //Create the functional group segment
        //        ediGroup.Set(ref oGroup, (ediGroup)oInterchange.CreateGroup("004010X092A1"));
        //        ediDataSegment.Set(ref oSegment, (ediDataSegment)oGroup.GetDataSegmentHeader());
        //        oSegment.set_DataElementValue(1, 0, "HS");
        //        oSegment.set_DataElementValue(2, 0, "T00000000020315");
        //        oSegment.set_DataElementValue(3, 0, "RXHUB");//Receiver ID
        //        oSegment.set_DataElementValue(4, 0, Convert.ToString(gloDateMaster.gloDate.DateAsNumber(DateTime.Now.ToShortDateString())).Trim());
        //        string GS_Time = Convert.ToString(gloDateMaster.gloTime.TimeAsNumber(DateTime.Now.ToLocalTime().ToShortTimeString())).Trim();
        //        oSegment.set_DataElementValue(5, 0, FormattedTime(GS_Time).Trim());
        //        oSegment.set_DataElementValue(6, 0, "1");//"000000001"-------claredi------GS*HS*T00000000000109*RXHUB*20090418*1135*000000001*X*004010X092A1~       //H10014:Leading zeros detected in GS06. The X12 syntax requires the suppression of leading zeros for numeric elements
        //        oSegment.set_DataElementValue(7, 0, "X");
        //        oSegment.set_DataElementValue(8, 0, "004010X092A1");

        //        #endregion " Functional Group "

        //        #region "Transaction Set "
        //        //HEADER
        //        //ST TRANSACTION SET HEADER
        //        ediTransactionSet.Set(ref oTransactionset, (ediTransactionSet)oGroup.CreateTransactionSet("270"));
        //        ediDataSegment.Set(ref oSegment, (ediDataSegment)oTransactionset.GetDataSegmentHeader());
        //        oSegment.set_DataElementValue(2, 0, "000000001");

        //        #endregion "Transaction Set "

        //        #region " BHT "

        //        //Begining Segment 
        //        ediDataSegment.Set(ref oSegment, (ediDataSegment)oTransactionset.CreateDataSegment("BHT"));
        //        oSegment.set_DataElementValue(1, 0, "0022");
        //        oSegment.set_DataElementValue(2, 0, "13");//13=Request
        //        BHT_TransactionRef = DateTime.Now.Date.ToString("MMddyyyy") + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + DateTime.Now.Millisecond.ToString(); //Convert.ToString(gloDateMaster.gloDate.DateAsNumber(DateTime.Now.ToShortDateString())) + Convert.ToString(gloDateMaster.gloTime.TimeAsNumber(DateTime.Now.ToLocalTime().ToShortTimeString())).Trim();
        //        oSegment.set_DataElementValue(3, 0, BHT_TransactionRef);//"000000001"Submitter Transaction Identifier
        //        oSegment.set_DataElementValue(4, 0, Convert.ToString(gloDateMaster.gloDate.DateAsNumber(DateTime.Now.ToShortDateString())));
        //        string BHT_Time = Convert.ToString(gloDateMaster.gloTime.TimeAsNumber(DateTime.Now.ToLocalTime().ToShortTimeString())).Trim();
        //        oSegment.set_DataElementValue(5, 0, FormattedTime(BHT_Time).Trim());

        //        #endregion " BHT "

        //        #region " Information Source "

        //        ediDataSegment.Set(ref oSegment, (ediDataSegment)oTransactionset.CreateDataSegment("HL\\HL"));
        //        oSegment.set_DataElementValue(1, 0, "1");
        //        oSegment.set_DataElementValue(3, 0, "20");//20=Information Source
        //        oSegment.set_DataElementValue(4, 0, "1");

        //        //INFORMATION SOURCE NAME 
        //        ediDataSegment.Set(ref oSegment, (ediDataSegment)oTransactionset.CreateDataSegment("HL\\NM1\\NM1"));

        //        oSegment.set_DataElementValue(1, 0, "2B");//PR=Payer
        //        oSegment.set_DataElementValue(2, 0, "2");//2=Non-Person Entity
        //        oSegment.set_DataElementValue(3, 0, "RXHUB");//"INFORMATION SOURCE NAME" );//"PBM"
        //        oSegment.set_DataElementValue(8, 0, "PI");//PI=Payer Identification
        //        oSegment.set_DataElementValue(9, 0, "RXHUB");//PayerID


        //        #endregion " Information Source "

        //        #region " Receiver Loop "

        //        //INFORMATION RECEIVER LEVEL
        //        ediDataSegment.Set(ref oSegment, (ediDataSegment)oTransactionset.CreateDataSegment("HL(2)\\HL"));	//oSegment = (ediDataSegment) oTransactionset.CreateDataSegment(sN1Loop + "N1");
        //        oSegment.set_DataElementValue(1, 0, "2");
        //        oSegment.set_DataElementValue(2, 0, "1");
        //        oSegment.set_DataElementValue(3, 0, "21");//21=Information Receiver
        //        oSegment.set_DataElementValue(4, 0, "1");//1=Additional Subordinate HL Data segment in this Herarchical structure

        //        //INFORMATION RECEIVER NAME (It is the medical service Provider)
        //        ediDataSegment.Set(ref oSegment, (ediDataSegment)oTransactionset.CreateDataSegment("HL(2)\\NM1\\NM1"));	//oSegment = (ediDataSegment) oTransactionset.CreateDataSegment(sN1Loop + "N1");
        //        oSegment.set_DataElementValue(1, 0, "1P");//1P=Provider
        //        oSegment.set_DataElementValue(2, 0, "1");//1=Person

        //        //oSegment.set_DataElementValue(3, 0, "Provider L Name");//Provider  LastName
        //        oSegment.set_DataElementValue(3, 0, oPatient.Provider.ProviderLastName);//"JONES"//Provider  LastName

        //        //oSegment.set_DataElementValue(4, 0, "Provider F Name");//Provider FirstName
        //        oSegment.set_DataElementValue(4, 0, oPatient.Provider.ProviderFirstName);//"MARK"//Provider FirstName

        //        oSegment.set_DataElementValue(5, 0, oPatient.Provider.ProviderMiddleName);//"MARK"//Provider FirstName


        //        oSegment.set_DataElementValue(6, 0, "");
        //        oSegment.set_DataElementValue(7, 0, "");


        //        oSegment.set_DataElementValue(8, 0, "XX");//SV=Service Provider Number-------claredi------NM1*1P*1*Shette*Dharmaji***MD*SV*5463~        H21084:Invalid qualifier 'SV' found in '2100B NM108'. Only 'XX' is valid because the National Provider Identifier (NPI) is now mandated for use.

        //        //oSegment.set_DataElementValue(9, 0, "DEA Number");//Service Provider No
        //        oSegment.set_DataElementValue(9, 0, "1679678759");////oPatient.Provider.ProviderDEA);//"6666666"//Service Provider No

        //        //INFORMATION RECEIVER ADDITIONAL IDENTIFICATION
        //        ediDataSegment.Set(ref oSegment, (ediDataSegment)oTransactionset.CreateDataSegment("HL(2)\\NM1\\REF"));
        //        oSegment.set_DataElementValue(1, 0, "EO");
        //        oSegment.set_DataElementValue(2, 0, "T00000000020315");

        //        //INFORMATION RECEIVER ADDRESS
        //        ediDataSegment.Set(ref oSegment, (ediDataSegment)oTransactionset.CreateDataSegment("HL(2)\\NM1\\N3"));
        //        string ProviderAddres = "";
        //        ProviderAddres = oPatient.Provider.ProviderAddress.AddressLine1 + " " + oPatient.Provider.ProviderAddress.AddressLine2;
        //        // oSegment.set_DataElementValue(1, 0, "Provider Address");
        //        oSegment.set_DataElementValue(1, 0, ProviderAddres.Trim());//""

        //        //INFORMATION RECEIVER CITY/STATE/ZIP CODE
        //        ediDataSegment.Set(ref oSegment, (ediDataSegment)oTransactionset.CreateDataSegment("HL(2)\\NM1\\N4"));
        //        //oSegment.set_DataElementValue(1, 0, "Provider City");
        //        oSegment.set_DataElementValue(1, 0, oPatient.Provider.ProviderAddress.City);//""

        //        //oSegment.set_DataElementValue(2, 0, "Provider State");
        //        oSegment.set_DataElementValue(2, 0, oPatient.Provider.ProviderAddress.State);//""

        //        //oSegment.set_DataElementValue(3, 0, "Provider Zip");
        //        oSegment.set_DataElementValue(3, 0, oPatient.Provider.ProviderAddress.Zip);//-------claredi------N4*New york*Mi*43324~      H50002:Invalid State/Province Code ('Mi')       B51124:This Zip Code is not valid for this State.

        //        #endregion " Receiver Loop "

        //        #region " Subscriber Loop "

        //        //SUBSCRIBER LEVEL
        //        ediDataSegment.Set(ref oSegment, (ediDataSegment)oTransactionset.CreateDataSegment("HL(3)\\HL"));	//oSegment = (ediDataSegment) oTransactionset.CreateDataSegment(sN1Loop + "N1");
        //        oSegment.set_DataElementValue(1, 0, "3");
        //        oSegment.set_DataElementValue(2, 0, "2");
        //        oSegment.set_DataElementValue(3, 0, "22");//22=Subscriber
        //        oSegment.set_DataElementValue(4, 0, "0");//0=No Subordinate HL Data segment in this Herarchical structure

        //        //SUBSCRIBER TRACE NUMBER
        //        //ediDataSegment.Set(ref oSegment, (ediDataSegment)oTransactionset.CreateDataSegment("HL(3)\\TRN"));	//oSegment = (ediDataSegment) oTransactionset.CreateDataSegment(sN1Loop + "N1");
        //        //oSegment.set_DataElementValue(1, 0, "1");//1=Current Transaction Trace Numbers
        //        //oSegment.set_DataElementValue(2, 0, "93175-012547");//Reference ID
        //        //oSegment.set_DataElementValue(3, 0, "9967833434");//Originating Company ID


        //        //SUBSCRIBER NAME(A person who can be uniquely identified to an information source. Traditionally referred to as a member.)
        //        ediDataSegment.Set(ref oSegment, (ediDataSegment)oTransactionset.CreateDataSegment("HL(3)\\NM1\\NM1"));	//oSegment = (ediDataSegment) oTransactionset.CreateDataSegment(sN1Loop + "N1");
        //        oSegment.set_DataElementValue(1, 0, "IL");//IL=Insured or Subscriber
        //        oSegment.set_DataElementValue(2, 0, "1"); //1=Person
        //        gloRxHub.ClsSubscriber oSubscriber = new gloRxHub.ClsSubscriber();
        //        if (oPatient.Subscriber.Count > 0)
        //        {
        //            for (int i = 0; i <= oPatient.Subscriber.Count - 1; i++)
        //            {
        //                if (oPatient.Subscriber[i].SubscriberFirstName != "")
        //                {
        //                    //SLR: Free previously allocated osubscriber and then assign here and have a boolean that, it should not be disposed at later since it is a reference.
        //                    if (oSubscriber != null)
        //                    {
        //                        oSubscriber = null;
        //                    }
        //                    oSubscriber = oPatient.Subscriber[i];
        //                    IsoSubscriberReferenced = true;
        //                    break;
        //                }

        //            }

        //        }
        //        //                oSegment.set_DataElementValue(3, 0, "Subscriber Last Name");
        //        oSegment.set_DataElementValue(3, 0, oSubscriber.SubscriberLastName);//"PALTROW"

        //        //oSegment.set_DataElementValue(4, 0, "Subscriber First Name");
        //        oSegment.set_DataElementValue(4, 0, oSubscriber.SubscriberFirstName);//"BRUCE"

        //        oSegment.set_DataElementValue(5, 0, "");
        //        oSegment.set_DataElementValue(8, 0, "ZZ");

        //        //oSegment.set_DataElementValue(9, 0, "SubscriberPrimaryID");
        //        if (oSubscriber.SubscriberID.Trim() != "")
        //        {
        //            oSegment.set_DataElementValue(9, 0, oSubscriber.SubscriberID.Trim());
        //            oSegment.set_DataElementValue(9, 0, oPatient.SSN);
        //        }


        //        #endregion " Subscriber Loop "

        //        #region " Subscriber Additional Identification Loop "

        //        //SUBSCRIBER ADDITIONAL IDENTIFICATION

        //        if (oPatient.SSN.Trim() != "")
        //        {
        //            ediDataSegment.Set(ref oSegment, (ediDataSegment)oTransactionset.CreateDataSegment("HL(3)\\NM1\\REF"));

        //            oSegment.set_DataElementValue(1, 0, "SY");//SY=SS Number
        //            oSegment.set_DataElementValue(2, 0, oPatient.SSN.Trim());  //SSN

        //        }
        //        //SUBSCRIBER ADDRESS
        //        ediDataSegment.Set(ref oSegment, (ediDataSegment)oTransactionset.CreateDataSegment("HL(3)\\NM1\\N3"));	//oSegment = (ediDataSegment) oTransactionset.CreateDataSegment(sN1Loop + "N1");

        //        //oSegment.set_DataElementValue(1, 0, "Address1");//"Subscriber Address");
        //        string SubscriberAddres = "";
        //        SubscriberAddres = oSubscriber.SubscriberAddress.AddressLine1 + " " + oSubscriber.SubscriberAddress.AddressLine2;

        //        //oSegment.set_DataElementValue(1, 0, "Address1");//"Subscriber Address");
        //        oSegment.set_DataElementValue(1, 0, SubscriberAddres);//"2645 MULBERRY LANE"//"Subscriber Address");


        //        //SUBSCRIBER CITY,STATE and ZIP
        //        ediDataSegment.Set(ref oSegment, (ediDataSegment)oTransactionset.CreateDataSegment("HL(3)\\NM1\\N4"));

        //        //oSegment.set_DataElementValue(1, 0, "City");//"City");
        //        oSegment.set_DataElementValue(1, 0, oSubscriber.SubscriberAddress.City);//"TOLEDO"//"City");

        //        //oSegment.set_DataElementValue(2, 0, "State");//"State");
        //        oSegment.set_DataElementValue(2, 0, oSubscriber.SubscriberAddress.State);//"OH"//"State");

        //        //                oSegment.set_DataElementValue(3, 0, "Zip");//"ZIP");
        //        oSegment.set_DataElementValue(3, 0, oSubscriber.SubscriberAddress.Zip);//"54360"//"ZIP");
        //        oSegment.set_DataElementValue(4, 0, "US");//"County");

        //        //SUBSCRIBER DEMOGRAPHIC INFORMATION
        //        ediDataSegment.Set(ref oSegment, (ediDataSegment)oTransactionset.CreateDataSegment("HL(3)\\NM1\\DMG"));
        //        oSegment.set_DataElementValue(1, 0, "D8");//D8=Date Expressed in Format CCYYMMDD
        //        string _patDOB = gloDateMaster.gloDate.DateAsNumber(oPatient.DOB.ToShortDateString()).ToString();
        //        oSegment.set_DataElementValue(2, 0, _patDOB);// oPatient.DOB.ToString()); //"19450201"//Date of Birth
        //        //oSegment.set_DataElementValue(2, 0, oPatient.DOB.ToString()); //Date of Birth

        //        //oSegment.set_DataElementValue(3, 0, "Gender"); //Gender
        //        if (oPatient.Gender != "")
        //        {
        //            if (oPatient.Gender == "Female")
        //            {
        //                oSegment.set_DataElementValue(3, 0, "F");//"M" //Gender
        //            }
        //            else if (oPatient.Gender == "Male")
        //            {
        //                oSegment.set_DataElementValue(3, 0, "M");//"M" //Gender
        //            }
        //            else if (oPatient.Gender == "Other")
        //            {
        //                oSegment.set_DataElementValue(3, 0, "O");//"Other"
        //            }
        //        }
        //        else
        //        {
        //            oSegment.set_DataElementValue(3, 0, "");//"M" //Gender
        //        }


        //        //SUBSCRIBER DATE
        //        ediDataSegment.Set(ref oSegment, (ediDataSegment)oTransactionset.CreateDataSegment("HL(3)\\NM1\\DTP"));
        //        oSegment.set_DataElementValue(1, 0, "472");//472=Service,102=Issue,307=Eligibility,435=Admission
        //        oSegment.set_DataElementValue(2, 0, "D8");//D8=Date Expressed in Format CCYYMMDD
        //        oSegment.set_DataElementValue(3, 0, Convert.ToString(gloDateMaster.gloDate.DateAsNumber(DateTime.Now.ToShortDateString())));//"20020801"//"Service DATE");//Service Date //Statement Date //Admission date/hour // Discharge Hour

        //        //SUBSCRIBER ELIGIBILITY OR BENEFIT INQUIRY INFORMATION
        //        ediDataSegment.Set(ref oSegment, (ediDataSegment)oTransactionset.CreateDataSegment("HL(3)\\NM1\\EQ\\EQ"));
        //        oSegment.set_DataElementValue(1, 0, "88"); // Pharmacy (recommended by RxHub)

        //        ediDataSegment.Set(ref oSegment, (ediDataSegment)oTransactionset.CreateDataSegment("HL(3)\\NM1\\EQ\\EQ"));
        //        oSegment.set_DataElementValue(1, 0, "90");//Mail Order Prescription Drug

        //        #endregion " Subscriber Loop "

        //        #region  " Save EDI File "

        //        //Save to a file
        //        oEdiDoc.Save(sPath + sEdiFile);
        //        string EdiFile = "";

        //        EdiFile = sPath + sEdiFile;

        //        #endregion  " Save EDI File "

        //        #region  " Save 270 Info to database "
        //        //SLR: based on previously allocated memory, disconnect, free it, and then allocate once more
        //        if (oclsgloRxHubDBLayer != null)
        //        {
        //            oclsgloRxHubDBLayer.Disconnect();
        //            oclsgloRxHubDBLayer.Dispose();
        //            oclsgloRxHubDBLayer = null;
        //        }
        //        oclsgloRxHubDBLayer = new clsgloRxHubDBLayer();
        //        oclsgloRxHubDBLayer.Connect(ClsgloRxHubGeneral.ConnectionString);
        //        //Message ID       //TransactionID 
        //        oclsgloRxHubDBLayer.InsertEDIResquest270("gsp_InUpRxH_270Request_Details", strControlNo, BHT_TransactionRef, oPatient);

        //        #endregion  " Save 270 Info to database "
        //        //SLR: Disconnect and free oclsgloRxHubDBLayer
        //        if (oclsgloRxHubDBLayer != null)
        //        {
        //            oclsgloRxHubDBLayer.Disconnect();
        //            oclsgloRxHubDBLayer.Dispose();
        //            oclsgloRxHubDBLayer = null;
        //        }
        //        //SLR: Finally based on boolean, free oSubscriber
        //        if (IsoSubscriberReferenced)
        //        {
        //            if (oSubscriber != null)
        //            {
        //                oSubscriber.Dispose();
        //                oSubscriber = null;
        //            }
        //        }
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        gloAuditTrail.gloAuditTrail.ExceptionLog(gloAuditTrail.ActivityModule.Prescription, gloAuditTrail.ActivityCategory.CreatePrescription, gloAuditTrail.ActivityType.General, ex.ToString(), gloAuditTrail.ActivityOutCome.Failure);
        //        throw ex;
        //        //return false;

        //    }
            
           
        //}


        public void LoadEDIObject_old()
        {
            ediDocument oEdiDoc = null;
            //ediInterchange oInterchange = null;
            //ediGroup oGroup = null;
            //ediTransactionSet oTransactionset = null;
            //ediDataSegment oSegment = null;
            ediSchema oSchema = null;
            ediSchemas oSchemas = null;
            //ediAcknowledgment oAck = null;
            try
            {
                sPath = AppDomain.CurrentDomain.BaseDirectory;
                sSEFFile = "270_X092A1.SEF";
                sEdiFile = "270OUTPUT.x12";
                oEdiDoc = new ediDocument();
                ediDocument.Set(ref oEdiDoc, new ediDocument());
                //oEdiDoc = new ediDocument();
                ediSchemas.Set(ref oSchemas, (ediSchemas)oEdiDoc.GetSchemas());
                oSchemas.EnableStandardReference = false;

                //oEdiDoc.SegmentTerminator = "~\r\n";
                //oEdiDoc.ElementTerminator = "*";
                //oEdiDoc.CompositeTerminator = ":";

                oEdiDoc.CursorType = DocumentCursorTypeConstants.Cursor_ForwardWrite;

                oEdiDoc.set_Property(DocumentPropertyIDConstants.Property_DocumentBufferIO, 2000);

                ediSchema.Set(ref oSchema, (ediSchema)oEdiDoc.LoadSchema("270_X092A1.SEF", 0));

                System.IO.FileInfo ofile = new System.IO.FileInfo(sPath + sSEFFile);
                if (ofile.Exists == false)
                {
                    // // MessageBox.Show("SEF file is not present in the base directory.  ", _messageboxcaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

            }
            catch (Exception ex)
            {
                gloAuditTrail.gloAuditTrail.ExceptionLog(gloAuditTrail.ActivityModule.Prescription, gloAuditTrail.ActivityCategory.CreatePrescription, gloAuditTrail.ActivityType.General, ex.ToString(), gloAuditTrail.ActivityOutCome.Failure);
            }
        }

        //public void LoadEDIObject()
        //{
        //    try
        //    {
        //        //SLR: Chk
        //        ediDocument.Set(ref oEdiDoc, new ediDocument());  //SLR: When is new ediDcoument freeded?  // oEdiDoc = new ediDocument();
        //        sPath = AppDomain.CurrentDomain.BaseDirectory;
        //        sSEFFile = "271_X092A1.SEF";
        //        sEdiFile = "271.X12";
        //        // Disabling the internal standard reference library to makes sure that 
        //        // FREDI uses only the SEF file provided
        //        ediSchemas.Set(ref oSchemas, (ediSchemas)oEdiDoc.GetSchemas());    //oSchemas = (ediSchemas) oEdiDoc.GetSchemas();
        //        oSchemas.EnableStandardReference = false;

        //        // This makes certain that the EDI file must use the same version SEF file, otherwise
        //        // the process will stop.
        //        oSchemas.set_Option(SchemasOptionIDConstants.OptSchemas_VersionRestrict, 1);

        //        // By setting the cursor type to ForwardOnly, FREDI does not load the entire file into memory, which
        //        // improves performance when processing larger EDI files.
        //        oEdiDoc.CursorType = DocumentCursorTypeConstants.Cursor_ForwardOnly;

        //        // If an acknowledgment file has to be generated, an acknowledgment object must be created, and its 
        //        // property must be enabled before loading the EDI file.
        //        oAck = (ediAcknowledgment)oEdiDoc.GetAcknowledgment();
        //        oAck.EnableFunctionalAcknowledgment = true;

        //        // Set the starting point of the control numbers in the acknowledgment
        //        oAck.set_Property(AcknowledgmentPropertyIDConstants.PropertyAck_StartInterchangeControlNum, 1001);
        //        oAck.set_Property(AcknowledgmentPropertyIDConstants.PropertyAck_StartGroupControlNum, 1);
        //        oAck.set_Property(AcknowledgmentPropertyIDConstants.PropertyAck_StartTransactionSetControlNum, 1);

        //        // Error codes that are not automatically mapped to an acknowlegment error number by FREDI can be set by
        //        // using the MapDataElementLevelError method.
        //        oAck.MapDataElementLevelError(13209, 5, "", "", "", "");

        //        oEdiDoc.LoadSchema(sSEFFile, 0);
        //        oEdiDoc.LoadSchema("997_X12-4010.SEF", 0);
        //        //ediSchema.Set(ref oSchemas, oEdiDoc.LoadSchema("997_X12-4010.SEF", 0));	//for Ack (997) file

        //        // Loads EDI file and the corresponding SEF file
        //        //OpenFileDialog oDialog = new OpenFileDialog();
        //        //if (oDialog.ShowDialog() == DialogResult.OK)
        //        //{
        //        //    string _FileName = "";
        //        //    _FileName = oDialog.FileName;
        //        //    if (System.IO.File.Exists(_FileName) == true)
        //        //    {
        //        //        sEdiFile = _FileName;
        //        //        oEdiDoc.LoadEdi(sEdiFile);
        //        //    }
        //        //}
        //        //SLR: Free previously allocated memory and then allocate aggain
        //        FreeEDIObject(ref oEdiDoc);
        //        oEdiDoc = new ediDocument();
        //        // sEdiFile = "EligibilityResponse.X12";
        //        //sEdiFile = "SingleCoverage.X12";
        //        //sEdiFile = "MultipleCoverage.X12";
        //        //sEdiFile = "PayeDependantatPayer.X12";
        //        //sEdiFile = "PatientNotFound.X12";
        //       // sEdiFile = "ErrorAtPBM.X12";
        //        sEdiFile = "271Responce_PDF.X12";
        //        oEdiDoc.LoadEdi(sPath + sEdiFile);
        //    }
        //    catch (Exception ex)
        //    {
        //        gloAuditTrail.gloAuditTrail.ExceptionLog(gloAuditTrail.ActivityModule.Prescription, gloAuditTrail.ActivityCategory.CreatePrescription, gloAuditTrail.ActivityType.General, ex.ToString(), gloAuditTrail.ActivityOutCome.Failure);
        //        throw ex;
        //    }
        //}

        public bool LoadEDIObject_271(string _271FilePath)
        {
            ediDocument oEdiDoc = null;
            ediSchemas oSchemas = null;
            ediAcknowledgment oAck = null;
            try
            {
                //SLR: Free previously allocated memory and then allocate aggain
                FreeEDIObject(ref oEdiDoc);
                oEdiDoc = new ediDocument();
                ediDocument.Set(ref oEdiDoc, new ediDocument()); //SLR: When is thsi new edidocmetn freed?  // oEdiDoc = new ediDocument();
                sPath = AppDomain.CurrentDomain.BaseDirectory;
                sSEFFile = "271_X092A1.SEF";
                sEdiFile = "271.X12";
                // Disabling the internal standard reference library to makes sure that 
                // FREDI uses only the SEF file provided
                ediSchemas.Set(ref oSchemas, (ediSchemas)oEdiDoc.GetSchemas());    //oSchemas = (ediSchemas) oEdiDoc.GetSchemas();
                oSchemas.EnableStandardReference = false;

                // This makes certain that the EDI file must use the same version SEF file, otherwise
                // the process will stop.
                oSchemas.set_Option(SchemasOptionIDConstants.OptSchemas_VersionRestrict, 1);

                // By setting the cursor type to ForwardOnly, FREDI does not load the entire file into memory, which
                // improves performance when processing larger EDI files.
                oEdiDoc.CursorType = DocumentCursorTypeConstants.Cursor_ForwardOnly;

                // If an acknowledgment file has to be generated, an acknowledgment object must be created, and its 
                // property must be enabled before loading the EDI file.
                oAck = (ediAcknowledgment)oEdiDoc.GetAcknowledgment();
                oAck.EnableFunctionalAcknowledgment = true;

                // Set the starting point of the control numbers in the acknowledgment
                oAck.set_Property(AcknowledgmentPropertyIDConstants.PropertyAck_StartInterchangeControlNum, 1001);
                oAck.set_Property(AcknowledgmentPropertyIDConstants.PropertyAck_StartGroupControlNum, 1);
                oAck.set_Property(AcknowledgmentPropertyIDConstants.PropertyAck_StartTransactionSetControlNum, 1);

                // Error codes that are not automatically mapped to an acknowlegment error number by FREDI can be set by
                // using the MapDataElementLevelError method.
                oAck.MapDataElementLevelError(13209, 5, "", "", "", "");

                oEdiDoc.LoadSchema(sPath + sSEFFile, 0);
                oEdiDoc.LoadSchema(sPath +  "997_X12-4010.SEF", 0);
                //ediSchema.Set(ref oSchemas, oEdiDoc.LoadSchema("997_X12-4010.SEF", 0));	//for Ack (997) file
                
                oEdiDoc.LoadEdi(_271FilePath);

                return true;
            }
            catch (Exception ex)
            {
                gloAuditTrail.gloAuditTrail.ExceptionLog(gloAuditTrail.ActivityModule.Prescription, gloAuditTrail.ActivityCategory.CreatePrescription, gloAuditTrail.ActivityType.General, ex.ToString(), gloAuditTrail.ActivityOutCome.Failure);
                return false;
                throw ex;
               

            }
            //SLR: Finaly free if oedidoc is not used again
        }

        public bool LoadEDIObject_270()
        {
            ediDocument oEdiDoc = null;
            //ediInterchange oInterchange = null;
            //ediGroup oGroup = null;
            //ediTransactionSet oTransactionset = null;
            //ediDataSegment oSegment = null;
            ediSchema oSchema = null;
            ediSchemas oSchemas = null;
            //ediAcknowledgment oAck = null;
            try
            {
                sPath = AppDomain.CurrentDomain.BaseDirectory;

                sSEFFile = "270_X092A1.SEF";
                //sEdiFile = "270OUTPUT.x12";

                //strEDIFileName = DateTime.Now.Date.ToString("MMddyyyy") + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + DateTime.Now.Millisecond.ToString() + ".X12"; //Convert.ToString(gloDateMaster.gloDate.DateAsNumber(DateTime.Now.ToShortDateString())) + Convert.ToString(gloDateMaster.gloTime.TimeAsNumber(DateTime.Now.ToLocalTime().ToShortTimeString())).Trim();
                sEdiFile = "EDI270_" + DateTime.Now.Date.ToString("MMddyyyy") + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + DateTime.Now.Millisecond.ToString() + ".X12"; //Convert.ToString(gloDateMaster.gloDate.DateAsNumber(DateTime.Now.ToShortDateString())) + Convert.ToString(gloDateMaster.gloTime.TimeAsNumber(DateTime.Now.ToLocalTime().ToShortTimeString())).Trim();
                //SLR: Free previously allocated memory and then allocate aggain
                FreeEDIObject(ref oEdiDoc);
                oEdiDoc = new ediDocument();
                ediDocument.Set(ref oEdiDoc, new ediDocument()); //SLR: When is the new edidocument freedd?
                //oEdiDoc = new ediDocument();
                ediSchemas.Set(ref oSchemas, (ediSchemas)oEdiDoc.GetSchemas());
                oSchemas.EnableStandardReference = false;

                //oEdiDoc.SegmentTerminator = "~\r\n";
                //oEdiDoc.ElementTerminator = "*";
                //oEdiDoc.CompositeTerminator = ":";

                oEdiDoc.CursorType = DocumentCursorTypeConstants.Cursor_ForwardWrite;

                oEdiDoc.set_Property(DocumentPropertyIDConstants.Property_DocumentBufferIO, 2000);

                ediSchema.Set(ref oSchema, (ediSchema)oEdiDoc.LoadSchema("270_X092A1.SEF", 0));

                System.IO.FileInfo ofile = new System.IO.FileInfo(sPath + sSEFFile);
                if (ofile.Exists == false)
                {
                    MessageBox.Show("SEF file is not present in the base directory.  ", _messageboxcaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    if (ofile != null)
                    {
                        ofile = null;
                    }
                    return false;
                }
                if (ofile != null)
                {
                    ofile = null;
                }

                return true;
            }
            catch (Exception ex)
            {
                gloAuditTrail.gloAuditTrail.ExceptionLog(gloAuditTrail.ActivityModule.Prescription, gloAuditTrail.ActivityCategory.CreatePrescription, gloAuditTrail.ActivityType.General, ex.ToString(), gloAuditTrail.ActivityOutCome.Failure);
                return false;
            }
            //SLR: Finaly free oEDIDoc, if it is nt used in next tim
        }

        public void FreeEDIObject(ref ediDocument oEdiDoc)
        {
            if (oEdiDoc != null)
            {
                //try
                //{

                //    oEdiDoc.Close();
                //}
                //catch (Exception ex)
                //{

                //}
                try
                {
                    try
                    {
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(oEdiDoc);
                        System.Runtime.InteropServices.Marshal.FinalReleaseComObject(oEdiDoc);
                    }
                    catch //(Exception ex1)
                    {
 
                    }
                    try
                    {
                        oEdiDoc.Dispose();
                    }
                    catch //(Exception ex3)
                    {
                    }
                }                  
                catch //(Exception ex2)
                {

                }

                oEdiDoc = null;

            }
        }

        private string FormattedTime(string TimeFormat)
        {
            //SLR: just return TimeFormat.PadLeft(4,'0')) ;
            return TimeFormat.PadLeft(4, '0');

            //int _length = 0;
            //_length = TimeFormat.Length;
            //if (_length == 0)
            //{
            //    TimeFormat = "0000";
            //}
            //if (_length == 1)
            //{
            //    TimeFormat = "000" + TimeFormat;
            //}
            //else if (_length == 2)
            //{
            //    TimeFormat = "00" + TimeFormat;
            //}
            //else if (_length == 3)
            //{
            //    TimeFormat = "0" + TimeFormat;
            //}
            //else if (_length == 4)
            //{
            //    TimeFormat = TimeFormat;
            //}
            //return TimeFormat;
        }

                                      
        #endregion " Procedures And Functions "


        #region "Retrieve the patient formulary data"
        public DataTable GetFormularlys(Int64 nPatientID, DateTime dtdate)
        {
            DataTable dt =null; //SLR: new is not needed
            try
            {
                //SLR: disconnect and FRee previoisuly allocated memory and then allocate here
                if (oclsgloRxHubDBLayer != null)
                {
                    oclsgloRxHubDBLayer.Disconnect();
                    oclsgloRxHubDBLayer.Dispose();
                    oclsgloRxHubDBLayer = null;
                }
                oclsgloRxHubDBLayer = new clsgloRxHubDBLayer();
                oclsgloRxHubDBLayer.Connect(ClsgloRxHubGeneral.ConnectionString);
                string strSQL = "select distinct sMessageID,sSTLoopControlID,sPBM_PayerName,sFormularyListID,sAlternativeListID,sCoverageID,sCoPayID,dtResquestDateTimeStamp from RxH_271Master where nPatientID= " + nPatientID + " and dtResquestDateTimeStamp >='" + dtdate + "' order by dtResquestDateTimeStamp desc";
                dt = oclsgloRxHubDBLayer.GetFormularlyCheckInfo(strSQL);

                if (dt.Rows.Count > 0)
                {

                    return dt;

                }
                else
                {
                    return null;
                }



            }
            catch (Exception ex)
            {
                gloAuditTrail.gloAuditTrail.ExceptionLog(gloAuditTrail.ActivityModule.Prescription, gloAuditTrail.ActivityCategory.CreatePrescription, gloAuditTrail.ActivityType.General, ex.ToString(), gloAuditTrail.ActivityOutCome.Failure);
                throw ex;
                //return null;
            }
            //SLR: Finaly disconnect and free oclgloRxHubdblayer, if it is not used next tiem
        }
        
        public DataTable GetFormularlyCheckInfoWithoutPBM(Int64 nPatientID, string YesterdayDate)
        {
            DataTable dt = null; //SLR: new is not needed
            string strSQL = "";
            try
            {
                //SLR: disconnect and Free previously allocated mmeory before allocating againg
                if (oclsgloRxHubDBLayer != null)
                {
                    oclsgloRxHubDBLayer.Disconnect();
                    oclsgloRxHubDBLayer.Dispose();
                    oclsgloRxHubDBLayer = null;
                }
                oclsgloRxHubDBLayer = new clsgloRxHubDBLayer();
                oclsgloRxHubDBLayer.Connect(ClsgloRxHubGeneral.ConnectionString);
                

                if (YesterdayDate != "")
                {
                    //////query with yesterday date. since i m sending medhx today and i had sent eligibility yesterday. therefore there is a date problem while fetching 271 data
                    strSQL = "select sPBM_PayerParticipantID,sPBM_PayerName,sCardHolderID,sCardHolderName,sGroupID,sPBM_PayerMemberID from RxH_271Master where (isnull(sPhEligiblityorBenefitInfo,'') = 'Active Coverage' or isnull(sMailOrdEligiblityorBenefitInfo,'') = 'Active Coverage') and nPatientID= " + nPatientID + " and convert(datetime,convert(varchar,dtResquestDateTimeStamp,101)) = convert(datetime,convert(varchar,'" + YesterdayDate + "',101))  order by dtResquestDateTimeStamp desc";
                }
                else
                {
                    //////query with todays date 
                    strSQL = "select sPBM_PayerParticipantID,sPBM_PayerName,sCardHolderID,sCardHolderName,sGroupID,sPBM_PayerMemberID from RxH_271Master where (isnull(sPhEligiblityorBenefitInfo,'') = 'Active Coverage' or isnull(sMailOrdEligiblityorBenefitInfo,'') = 'Active Coverage') and nPatientID= " + nPatientID + " and convert(datetime,convert(varchar,dtResquestDateTimeStamp,101)) = convert(datetime,convert(varchar,'" + DateTime.Today + "',101))  order by dtResquestDateTimeStamp desc";
                }


                dt = oclsgloRxHubDBLayer.GetFormularlyCheckInfo(strSQL);

                if (dt.Rows.Count > 0)
                {
                    return dt;

                }
                else
                {
                    return null;
                }
            }
           
            catch (Exception ex)
            {
                gloAuditTrail.gloAuditTrail.ExceptionLog(gloAuditTrail.ActivityModule.Prescription, gloAuditTrail.ActivityCategory.CreatePrescription, gloAuditTrail.ActivityType.General, ex.ToString(), gloAuditTrail.ActivityOutCome.Failure);
                throw ex;
                //return null;
            }
            //SLR:FInaly disconnect and free if oclsglorxhubdblayer is not used next tiem
        }

        public DataTable GetFormularlyCheckInfowithPBM(Int64 nPatientID, string strPBM, string strPBMMemberID, string Yesterdaydate)
        {
            DataTable dt = null; //SLR: new is not needed
            string strSQL = "";
            try
            {
                //SLR: Disconnect and free, previously allocated memory and then allocate again
                if (oclsgloRxHubDBLayer != null)
                {
                    oclsgloRxHubDBLayer.Disconnect();
                    oclsgloRxHubDBLayer.Dispose();
                    oclsgloRxHubDBLayer = null;
                }
                oclsgloRxHubDBLayer = new clsgloRxHubDBLayer();
                oclsgloRxHubDBLayer.Connect(ClsgloRxHubGeneral.ConnectionString);                
                    //////query with todays date 
                    if (strPBM.Contains("-"))
                    {
                        strSQL = "select sPBM_PayerParticipantID,sPBM_PayerName,sCardHolderID,sCardHolderName,sGroupID,sPBM_PayerMemberID from RxH_271Master where nPatientID= " + nPatientID + " and convert(datetime,convert(varchar,dateadd(day,3,dtResquestDateTimeStamp),101)) > convert(datetime,convert(varchar,'" + DateTime.Today + "',101)) and isnull(sPBM_PayerName,'')+'-'+isnull(sHealthPlanName,'')= '" + strPBM + "' and isnull(sPBM_PayerMemberID,'') ='" + strPBMMemberID + "' order by dtResquestDateTimeStamp desc";
                    }
                    else
                    {
                        strSQL = "select sPBM_PayerParticipantID,sPBM_PayerName,sCardHolderID,sCardHolderName,sGroupID,sPBM_PayerMemberID from RxH_271Master where nPatientID= " + nPatientID + " and convert(datetime,convert(varchar,dateadd(day,3,dtResquestDateTimeStamp),101)) > convert(datetime,convert(varchar,'" + DateTime.Today + "',101)) and isnull(sPBM_PayerName,'')= '" + strPBM + "' and isnull(sPBM_PayerMemberID,'') ='" + strPBMMemberID + "' order by dtResquestDateTimeStamp desc";
                    }
               // }

                dt = oclsgloRxHubDBLayer.GetFormularlyCheckInfo(strSQL);

                if (dt.Rows.Count > 0)
                {

                    return dt;

                }
                else
                {
                    return null;
                }

            }
            catch (Exception ex)
            {
                gloAuditTrail.gloAuditTrail.ExceptionLog(gloAuditTrail.ActivityModule.Prescription, gloAuditTrail.ActivityCategory.CreatePrescription, gloAuditTrail.ActivityType.General, ex.ToString(), gloAuditTrail.ActivityOutCome.Failure);
                throw ex;
                //return null;
            }
            //SLR: Finaly Disconnect and free, oclsgloRXBudblayer, if it is not used next time 
        }

    

        public Cls271Information Translate271Response()
        {
            return null;
        }

        public Cls271Information Translate271Response_AutoEligibility()
        {
            return null;
        }


        public bool SaveEDIResponse271(ClsRxH_271Master objClsRxH_271Master)
        {
            clsgloRxHubDBLayer oclsgloRxHubDBLayer = new clsgloRxHubDBLayer();
            try
            {
                oclsgloRxHubDBLayer.Connect(ClsgloRxHubGeneral.ConnectionString);

                oclsgloRxHubDBLayer.InsertEDIResponse271_Details("gsp_InUpRxH_RxH_271Response_Details", objClsRxH_271Master);



                oclsgloRxHubDBLayer.InsertEDIResponse271_Master("gsp_InUpRxH_271Master", objClsRxH_271Master);

                if (objClsRxH_271Master.RxH_271Details.Count > 0)
                {
                    oclsgloRxHubDBLayer.InsertEDIResponse271_SubscriberDetails("gsp_InUpRxH_271Details", objClsRxH_271Master);
                }

                return true;
            }
            catch (Exception ex)
            {
                gloAuditTrail.gloAuditTrail.ExceptionLog(gloAuditTrail.ActivityModule.Prescription, gloAuditTrail.ActivityCategory.CreatePrescription, gloAuditTrail.ActivityType.General, ex.ToString(), gloAuditTrail.ActivityOutCome.Failure);
                throw ex;
                //return false;

            }
            finally
            {
                //SLR: Finally Disconnect and free, if oclsgloRxbdbkayer is not used next tiem
                if (oclsgloRxHubDBLayer != null)
                {
                    oclsgloRxHubDBLayer.Disconnect();
                    oclsgloRxHubDBLayer.Dispose();
                    oclsgloRxHubDBLayer = null;
                }
            }
            
        }
        #endregion "Retrieve the patient formulary data"



        internal byte[] ConvertFiletoBinaryEDI(string strFileName, Int16 MethodType, Int16 rbType)
        {
            if (File.Exists(strFileName))
            {
                FileStream oFile = default(FileStream);
                BinaryReader oReader = default(BinaryReader);
                DataSet ds= new DataSet(); 
                try
                {
                    //'Please uncomment the following line of code to read the file, even the file is in use by same or another process 
                    //oFile = New FileStream(strFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 8, FileOptions.Asynchronous) 

                    //'To read the file only when it is not in use by any process 
                    oFile = new FileStream(strFileName, FileMode.Open, FileAccess.Read);

                    oReader = new BinaryReader(oFile);
                    byte[] bytesRead = oReader.ReadBytes(Convert.ToInt32(oFile.Length));
                    byte[] readbytes = null;

                    //eRxService.eRxMessage myService = new eRxService.eRxMessage();
                    //myService.Credentials = System.Net.CredentialCache.DefaultCredentials;
                    //string _Key = myService.Login("sarika@ophit.net", "spX12ss@!!21nasik");

                    using (gloDatabaseLayer.DBLayer oDB = new gloDatabaseLayer.DBLayer(ClsgloRxHubGeneral.ConnectionString))
                    {
                        oDB.Connect(false);

                        oDB.Retrive_Query("SELECT ISNULL(sSettingsValue,'') AS sSettingsValue FROM Settings WITH (NOLOCK) WHERE UPPER(sSettingsName) in ('eRxStagingWebserviceURL','eRxProductionWebserviceURL') order by sSettingsName",out ds);

                        oDB.Disconnect();
                    }


                    if (ClsgloRxHubGeneral.gblnIsRxhubStagingServer == true)
                    {
                        eRxService.eRxMessage myService = new eRxService.eRxMessage();
                        myService.Credentials = System.Net.CredentialCache.DefaultCredentials;
                        myService.Url = ds.Tables[0].Rows[1]["sSettingsValue"].ToString(); 
                        string _Key = myService.Login("sarika@ophit.net", "spX12ss@!!21nasik");

                        if (MethodType == 1)
                        {
                            //readbytes = myService.PostClientRxMessage(bytesRead, _Key, "EDI270");
                        }
                        else if (MethodType == 2)
                        {
                            //readbytes = myService.PostRxHubMessage(bytesRead, _Key, "EDI270", rbType);
                        }
                        else if (MethodType == 0)
                        {
                            readbytes = myService.PostClientRxMessage(bytesRead, _Key, "EDI270");
                            //readbytes = myService.PostRxHubMessage(bytesRead, _Key, "EDI270", rbType);
                        }
                        //SLR: Free myService
                        if (myService != null)
                        {
                            myService.Dispose();
                            myService = null;
                        }
                        // readbytes = myService.PostRxHubMessage(bytesRead, _Key, "EDI270", 3) 
                    }
                    else
                    {
                        eRxServiceProd.eRxMessage myService = new eRxServiceProd.eRxMessage();
                        myService.Credentials = System.Net.CredentialCache.DefaultCredentials;
                        myService.Url = ds.Tables[0].Rows[0]["sSettingsValue"].ToString();
                        string _Key = myService.Login("sarika@ophit.net", "spX12ss@!!21nasik");

                        if (MethodType == 1)
                        {
                            //readbytes = myService.PostClientRxMessage(bytesRead, _Key, "EDI270");
                        }
                        else if (MethodType == 2)
                        {
                            //readbytes = myService.PostRxHubMessage(bytesRead, _Key, "EDI270", rbType);
                        }
                        else if (MethodType == 0)
                        {
                            readbytes = myService.PostClientRxMessage(bytesRead, _Key, "EDI270");
                            //readbytes = myService.PostRxHubMessage(bytesRead, _Key, "EDI270", rbType);
                        }
                        //SLR: Free myService
                        if (myService != null)
                        {
                            myService.Dispose();
                            myService = null;
                        }
                        // readbytes = myService.PostRxHubMessage(bytesRead, _Key, "EDI270", 3) 
                    }

                    return readbytes;
                }
                catch (IOException ex)
                {
                    gloAuditTrail.gloAuditTrail.ExceptionLog(gloAuditTrail.ActivityModule.Prescription, gloAuditTrail.ActivityCategory.CreatePrescription, gloAuditTrail.ActivityType.General, ex.ToString(), gloAuditTrail.ActivityOutCome.Failure);
                    //gloSurescriptGeneral.UpdateLog("") 
                    throw;
                    //return null;
                }
                catch (Exception ex)
                {
                    gloAuditTrail.gloAuditTrail.ExceptionLog(gloAuditTrail.ActivityModule.Prescription, gloAuditTrail.ActivityCategory.CreatePrescription, gloAuditTrail.ActivityType.General, ex.ToString(), gloAuditTrail.ActivityOutCome.Failure);
                    throw;
                    //return null;
                }
                finally
                {
                    if (ds != null)
                    {
                        ds.Dispose();
                        ds = null;
                    }
                    //SLR: Dispose, OFile and oReader
                    oFile.Close();
                    oFile.Dispose();
                    oReader.Close();
                    oReader.Dispose();

                }
            }
            else
            {
                return null;
            }
        }

        //create the Outbox folder if not present in the application startup path so that we can store the 270 eligiblity request files in it
        public bool CheckOutboxFolder(string strApplicationFilePath)
        {
            try
            {
                if (strApplicationFilePath != "")
                {
                    if (!Directory.Exists(strApplicationFilePath + "Outbox"))
                    {
                        //create the Outbox directory, to save the 270 generated files.
                        Directory.CreateDirectory(strApplicationFilePath + "Outbox");
                    }
                }
                else
                {
                    MessageBox.Show("Invalid application startup path.", "gloEMR", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                return true;
            }
            catch (Exception ex)
            {
                gloAuditTrail.gloAuditTrail.ExceptionLog(gloAuditTrail.ActivityModule.Prescription, gloAuditTrail.ActivityCategory.CreatePrescription, gloAuditTrail.ActivityType.General, ex.ToString(), gloAuditTrail.ActivityOutCome.Failure);
                MessageBox.Show(ex.ToString());
                return false;
            }
        }

        //create the Inbox folder if not present in the application startup path so that we can store the 271 eligiblity response files in it
        public bool CheckInboxFolder(string strApplicationFilePath)
        {
            try
            {
                if (strApplicationFilePath != "")
                {
                    if (!Directory.Exists(strApplicationFilePath + "Inbox"))
                    {
                        //create the Outbox directory, to save the 270 generated files.
                        Directory.CreateDirectory(strApplicationFilePath + "Inbox");
                    }
                }
                else
                {
                    MessageBox.Show("Invalid application startup path.", "gloEMR", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                return true;
            }
            catch (Exception ex)
            {
                gloAuditTrail.gloAuditTrail.ExceptionLog(gloAuditTrail.ActivityModule.Prescription, gloAuditTrail.ActivityCategory.CreatePrescription, gloAuditTrail.ActivityType.General, ex.ToString(), gloAuditTrail.ActivityOutCome.Failure);
                MessageBox.Show(ex.ToString());
                return false;
            }
        }

        internal string ExtractText(object cntFromDB)
        {
         //   MemoryStream stream = default(MemoryStream);
            string strfilename = "";
            string sAssemblyName = "";
            System.IO.FileStream oFile=null; 
            try
            {
                /////since the same func is called from RxSniffer, therefore if this func is called from gloEMR then show error message boxes else dont show them.
                sAssemblyName = System.Reflection.Assembly.GetCallingAssembly().FullName;

                if ((cntFromDB != null))
                {
                    byte[] content = (byte[])cntFromDB;
                   // stream = new MemoryStream(content);
                    
                    if (CheckInboxFolder(ClsgloRxHubGeneral.gnstrApplicationFilePath))
                    {
                        strfilename = ClsgloRxHubGeneral.gnstrApplicationFilePath + "Inbox\\" + "EDI271_"+ (System.DateTime.Now.ToString("yyyyMMddhhmmssmmm") + ".X12");
                        oFile = new System.IO.FileStream(strfilename, System.IO.FileMode.Create);
                        oFile.Write(content, 0, content.Length);
           //             stream.WriteTo(oFile);                        
                    }
                    

                    return strfilename;
                }
                else
                {
                    //MessageBox.Show("Response not available from Surescript Server", "gloEMR", MessageBoxButtons.OK, MessageBoxIcon.Information) 
                    //message changed as pwe discussion with yaw on 08 Dec 2008. 
                    if (sAssemblyName.Contains("gloEMR"))
                    {
                        System.Windows.Forms.MessageBox.Show("Did not receive any acknowledgement from RxHub Network!", "gloEMR", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    //gloSurescriptGeneral.UpdateLog("Response Object not returned") 
                    return "";
                }
            }
            catch (Exception ex)
            {
                gloAuditTrail.gloAuditTrail.ExceptionLog(gloAuditTrail.ActivityModule.Prescription, gloAuditTrail.ActivityCategory.CreatePrescription, gloAuditTrail.ActivityType.General, ex.ToString(), gloAuditTrail.ActivityOutCome.Failure);
                //gloSurescriptGeneral.UpdateLog("Error Extracting Response Object: " & ex.ToString) 
                //Throw New GloSurescriptException(ex.ToString) 
                throw;
            }
            finally
            {
                //if ((stream != null))
                //{
                //    stream.Dispose();
                //    stream = null;
                //}
                if (oFile != null)
                {                    
                    oFile.Close();
                    oFile.Dispose();
                    oFile = null; 
                }


            }
        }

        internal string ExtractXML(object cntFromDB)
        {
         //   MemoryStream stream = default(MemoryStream);
            try
            {
                if ((cntFromDB != null))
                {
                    byte[] content = (byte[])cntFromDB;
           //         stream = new MemoryStream(content);
                    CheckInboxFolder(ClsgloRxHubGeneral.gnstrApplicationFilePath);
                    string strfilename = gloSettings.FolderSettings.AppTempFolderPath + "Inbox\\" + (gloGlobal.clsFileExtensions.GetUniqueDateString("yyyyMMddHHmmssffff") +".xml");
                    System.IO.FileStream oFile = new System.IO.FileStream(strfilename, System.IO.FileMode.Create);
             //       stream.WriteTo(oFile);
                    oFile.Write(content, 0, content.Length);
                    oFile.Close();
                    oFile.Dispose();
                    oFile = null;
                    XmlDocument odoc = new XmlDocument();
                    odoc.Load(strfilename);
                    odoc = null;
                    return strfilename;
                }
                else
                {
                    //MessageBox.Show("Response not available from Surescript Server", "gloEMR", MessageBoxButtons.OK, MessageBoxIcon.Information) 
                    //message changed as pwe discussion with yaw on 08 Dec 2008. 
                    //MessageBox.Show("Did not recieve any acknowledgement from RxHub Network!", "gloEMR", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    //gloSurescriptGeneral.UpdateLog("Response Object not returned") 
                    return "";
                }
            }
            catch (Exception ex)
            {
                gloAuditTrail.gloAuditTrail.ExceptionLog(gloAuditTrail.ActivityModule.Prescription, gloAuditTrail.ActivityCategory.CreatePrescription, gloAuditTrail.ActivityType.General, ex.ToString(), gloAuditTrail.ActivityOutCome.Failure);
                //gloSurescriptGeneral.UpdateLog("Error Extracting Response Object: " & ex.ToString) 
                //Throw New GloSurescriptException(ex.ToString) 
                throw;
            }
            finally
            {
                //if ((stream != null))
                //{
                //    stream.Dispose();
                //    stream = null;
                //}                
            }
        }

        public string PostEDIFile(string strfilename, Int16 MethodType, Int16 rbType)
        {
            try
            {
                string strOutputfilename = ExtractText(ConvertFiletoBinaryEDI(strfilename, MethodType, rbType));
                return strOutputfilename;
            }
            catch (Exception ex)
            {
                gloAuditTrail.gloAuditTrail.ExceptionLog(gloAuditTrail.ActivityModule.Prescription, gloAuditTrail.ActivityCategory.CreatePrescription, gloAuditTrail.ActivityType.General, ex.ToString(), gloAuditTrail.ActivityOutCome.Failure);
                throw;
            }

        }

        public string PostEDIFile_New(string strfilename, Int16 MethodType, Int16 rbType)
        {
            try
            {
                string strOutputfilename = ExtractText(ConvertFiletoBinaryEDI(strfilename, 0, rbType));
                return strOutputfilename;
            }
            catch (Exception ex)
            {
                gloAuditTrail.gloAuditTrail.ExceptionLog(gloAuditTrail.ActivityModule.Prescription, gloAuditTrail.ActivityCategory.CreatePrescription, gloAuditTrail.ActivityType.General, ex.ToString(), gloAuditTrail.ActivityOutCome.Failure);
                throw;
            }

        }


        public string PostRxHistoryRequest(string strfilename, string RequestType)
        {
           
            string strOutputfilename = "";
            try
            {
                strOutputfilename = ExtractText(ConvertFiletoBinaryEDI_5010(strfilename, RequestType));
                return strOutputfilename;
            }
            catch (Exception ex)
            {
                gloAuditTrail.gloAuditTrail.ExceptionLog(gloAuditTrail.ActivityModule.Prescription, gloAuditTrail.ActivityCategory.CreatePrescription, gloAuditTrail.ActivityType.General, ex.ToString(), gloAuditTrail.ActivityOutCome.Failure);
                return strOutputfilename;

            }

        }


         #region "Rx History request and Response region "        
        public void GenerateRxMoreHistoryRequestwithPBM(string mPBM_PayerParticipantID, string mPBM_PayerName, string mCardHolderID, string mCardHolderName, string mGroupID, string mPBM_PayerMemberID,string RxHubParticipantId,string RxHubPassword)
        {
            string strRxHistoryResponseFileName = "";
            Int64 nPatientID;
            StringBuilder strInfo = new StringBuilder();
            try
            {
                string strOutbox = gloSettings.FolderSettings.AppTempFolderPath + "Outbox";

                //check and create directories if necessary 
                if (!Directory.Exists(strOutbox))
                {
                    //create it if it doesn't 
                    Directory.CreateDirectory(strOutbox);
                }

                //  string strRxHISResponseFileName = "RxHIS_" + System.DateTime.Now.Month.ToString("MM") + System.DateTime.Now.Day.ToString("dd") + System.DateTime.Now.Year.ToString("yyyy") + System.DateTime.Now.Hour.ToString("hh") + System.DateTime.Now.Minute.ToString("mm") + System.DateTime.Now.Second.ToString("ss") + ".xml";
                string strRxHISResponseFileName = "RxHIS_" + gloGlobal.clsFileExtensions.GetUniqueDateString("yyyyMMddHHmmssffff") + ".xml";


                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.NewLineOnAttributes = true;

                //create and update xml 
                strRxHistoryResponseFileName = strOutbox + "\\" + strRxHISResponseFileName;
                using (XmlWriter writer = XmlWriter.Create(strRxHistoryResponseFileName, settings))
                {
                        oPatient.RxH271Master.PayerParticipantId = mPBM_PayerParticipantID;
                        oPatient.RxH271Master.PayerName = mPBM_PayerName;
                        oPatient.RxH271Master.CardHolderId = mCardHolderID;
                        oPatient.RxH271Master.CardHolderName = mCardHolderName;
                        oPatient.RxH271Master.GroupId = mGroupID;
                        oPatient.RxH271Master.MemberID = mPBM_PayerMemberID;

                    nPatientID = oPatient.PatientID;
                    //main block Patient 
                    writer.WriteStartElement("Message", "http://www.ncpdp.org/schema/SCRIPT");
                    writer.WriteAttributeString("release", "001");
                    writer.WriteAttributeString("version", "008");


                    writer.WriteStartElement("Header");

                    writer.WriteStartElement("To");
                    writer.WriteAttributeString("Qualifier", "ZZZ");
                    writer.WriteValue(oPatient.RxH271Master.PayerParticipantId);
                    //writer.WriteValue("T00000000001000");



                    writer.WriteEndElement();

                    writer.WriteStartElement("From");
                    writer.WriteAttributeString("Qualifier", "ZZZ");
                    writer.WriteValue(RxHubParticipantId);//"T00000000020315"
                    writer.WriteEndElement();

                    writer.WriteElementString("MessageID", DateTime.Now.ToString("yyyyMMddhhmmss"));
                    //  writer.WriteElementString("SentTime", System.DateTime.Now.Year.ToString() + "-" + System.DateTime.Now.Month.ToString() + "-" + System.DateTime.Now.Day.ToString() + "T" + System.DateTime.Now.Hour.ToString() + ":" + System.DateTime.Now.Minute.ToString() + ":" + System.DateTime.Now.Second.ToString());
                    writer.WriteElementString("SentTime", System.DateTime.Now.ToString("yyyy-MM-dd") + "T" + System.DateTime.Now.ToString("hh:mm:ss"));


                    writer.WriteStartElement("Security");
                    writer.WriteStartElement("UsernameToken");
                    writer.WriteStartElement("Username");
                    writer.WriteEndElement();
                    writer.WriteEndElement();

                    writer.WriteStartElement("Sender");
                    writer.WriteElementString("SecondaryIdentification",RxHubPassword );//"FXTXGJVZ0W"
                    writer.WriteEndElement();

                    writer.WriteStartElement("Receiver");
                    writer.WriteElementString("SecondaryIdentification", "RxHUB");
                    writer.WriteEndElement();
                    writer.WriteEndElement();

                    writer.WriteElementString("TestMessage", "1");

                    writer.WriteEndElement();

                    writer.WriteStartElement("Body");
                    writer.WriteStartElement("RxHistoryRequest");

                    writer.WriteStartElement("Prescriber");
                    writer.WriteStartElement("Identification");
                    if (oPatient.Provider.ProviderDEA != "" && oPatient.Provider.ProviderNPI != "")
                    {
                        writer.WriteElementString("DEANumber", oPatient.Provider.ProviderDEA);//"1720049901"oPatient.Provider.ProviderDEA
                        writer.WriteElementString("NPI", oPatient.Provider.ProviderNPI);//"1720049901"oPatient.Provider.ProviderDEA
                    }
                    else if (oPatient.Provider.ProviderDEA != "")
                    {
                        writer.WriteElementString("DEANumber", oPatient.Provider.ProviderDEA);//"1720049901"oPatient.Provider.ProviderDEA
                    }
                    else if (oPatient.Provider.ProviderNPI != "")
                    {
                        writer.WriteElementString("NPI", oPatient.Provider.ProviderNPI);//"1720049901"oPatient.Provider.ProviderDEA
                    }
                    else
                    {
                        MessageBox.Show("Invalid medication history request will be posted since the provider does not have either DEA or NPI information", "gloEMR", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    writer.WriteEndElement();
                    //writer.WriteElementString("ClinicName", oPatient.Provider.ClinicName);
                    // ' writer.WriteEndElement() 

                    writer.WriteStartElement("Name");

                    if (oPatient.Provider.ProviderLastName.ToString().Length > 35)
                    {
                        strInfo.Append("Provider Last Name(35),");
                        writer.WriteElementString("LastName", oPatient.Provider.ProviderLastName.ToString().Substring(0, 35));

                    }
                    else
                    {
                        writer.WriteElementString("LastName", oPatient.Provider.ProviderLastName);
                    }
                    //writer.WriteElementString("LastName", oPatient.Provider.ProviderLastName);

                    if (oPatient.Provider.ProviderFirstName.ToString().Length > 35)
                    {
                        strInfo.Append("Provider First Name(35),");
                        writer.WriteElementString("FirstName", oPatient.Provider.ProviderFirstName.ToString().Substring(0, 35));
                    }
                    else
                    {
                        writer.WriteElementString("FirstName", oPatient.Provider.ProviderFirstName);
                    }
                    //writer.WriteElementString("FirstName", oPatient.Provider.ProviderFirstName);

                    writer.WriteElementString("MiddleName", oPatient.Provider.ProviderMiddleName);

                    //writer.WriteElementString("Prefix", oPatient.Provider.ProviderPrefix);
                    if (oPatient.Provider.ProviderPrefix.ToString().Length > 10)
                    {
                        strInfo.Append("Prefix(10),");
                        writer.WriteElementString("Prefix", oPatient.Provider.ProviderPrefix.ToString().Substring(0, 10));
                    }
                    else
                    {
                        writer.WriteElementString("Prefix", oPatient.Provider.ProviderPrefix);
                    }


                    writer.WriteEndElement();

                    writer.WriteStartElement("Address");

                    if (oPatient.Provider.ProviderAddress.AddressLine1.ToString().Length > 35)
                    {
                        strInfo.Append("Provider Address Line 1(35),");
                        writer.WriteElementString("AddressLine1", oPatient.Provider.ProviderAddress.AddressLine1.ToString().Substring(0, 35));
                    }
                    else
                    {
                        writer.WriteElementString("AddressLine1", oPatient.Provider.ProviderAddress.AddressLine1);
                    }

                    //writer.WriteElementString("AddressLine1", oPatient.Provider.ProviderAddress.AddressLine1);
                    if (oPatient.Provider.ProviderAddress.AddressLine2.ToString().Length > 35)
                    {
                        strInfo.Append("Provider Address Line 2(35),");
                        writer.WriteElementString("AddressLine2", oPatient.Provider.ProviderAddress.AddressLine2.ToString().Substring(0, 35));
                    }
                    else
                    {
                        writer.WriteElementString("AddressLine2", oPatient.Provider.ProviderAddress.AddressLine2);
                    }
                    //writer.WriteElementString("AddressLine2", oPatient.Provider.ProviderAddress.AddressLine2);

                    //writer.WriteElementString("City", oPatient.Provider.ProviderAddress.City);
                    if (oPatient.Provider.ProviderAddress.City.ToString().Length > 35)
                    {
                        strInfo.Append("Provider City(35),");
                        writer.WriteElementString("City", oPatient.Provider.ProviderAddress.City.ToString().Substring(0, 35));
                    }
                    else
                    {
                        writer.WriteElementString("City", oPatient.Provider.ProviderAddress.City);
                    }
                    writer.WriteElementString("State", oPatient.Provider.ProviderAddress.State);
                    writer.WriteElementString("ZipCode", oPatient.Provider.ProviderAddress.Zip);
                    //writer.WriteElementString("PlaceLocationQualifier", "");
                    writer.WriteEndElement();
                    
                    writer.WriteEndElement(); //Prescriber ends here

                    writer.WriteStartElement("Patient");
                    writer.WriteElementString("PatientRelationship", "1");

                    writer.WriteStartElement("Name");

                    if (oPatient.LastName.ToString().Length > 35)
                    {
                        strInfo.Append("Patient Last Name(35),");
                        writer.WriteElementString("LastName", oPatient.LastName.ToString().Substring(0, 35));

                    }
                    else
                    {
                        writer.WriteElementString("LastName", oPatient.LastName);
                    }

                    if (oPatient.FirstName.ToString().Length > 35)
                    {
                        strInfo.Append("Patient First Name(35),");
                        writer.WriteElementString("FirstName", oPatient.FirstName.ToString().Substring(0, 35));
                    }
                    else
                    {
                        writer.WriteElementString("FirstName", oPatient.FirstName);
                    }
                    //writer.WriteElementString("LastName", oPatient.LastName);
                    //writer.WriteElementString("FirstName", oPatient.FirstName);
                    writer.WriteElementString("MiddleName", oPatient.MiddleName);
                    writer.WriteEndElement();

                    string sgender = oPatient.Gender;
                    if (sgender == "Male")
                    {
                        sgender = "M";
                    }
                    else
                    {
                        if (sgender == "Female")
                        {
                            sgender = "F";
                        }
                        else
                        {
                            sgender = "U";
                        }
                    }
                    writer.WriteElementString("Gender", sgender);
                    System.String dateOfBirth = "";
                    //DateTime formatedDOB = default(DateTime);
                    //formatedDOB = Convert.ToDateTime(dateOfBirth.Year + "-" + dateOfBirth.Month + "-" + dateOfBirth.Day);

                    //dateOfBirth = oPatient.DOB.Year.ToString() + "-" + oPatient.DOB.Month.ToString() + "-" + oPatient.DOB.Day.ToString();
                    dateOfBirth = oPatient.DOB.ToString("yyyy-MM-dd");

                    writer.WriteElementString("DateOfBirth", dateOfBirth);
                    //oPatient.DOB) 

                    writer.WriteStartElement("Address");

                    if (oPatient.PatientAddress.AddressLine1.ToString().Length > 35)
                    {
                        strInfo.Append("Patient Address Line 1(35),");
                        writer.WriteElementString("AddressLine1", oPatient.PatientAddress.AddressLine1.ToString().Substring(0, 35));
                    }
                    else
                    {
                        writer.WriteElementString("AddressLine1", oPatient.PatientAddress.AddressLine1);
                    }


                    //writer.WriteElementString("AddressLine1", oPatient.PatientAddress.AddressLine1);
                    //writer.WriteElementString("AddressLine2", oPatient.PatientAddress.AddressLine2);
                    if (oPatient.PatientAddress.City.ToString().Length > 35)
                    {
                        strInfo.Append("Patient City(35),");
                        writer.WriteElementString("City", oPatient.PatientAddress.City.ToString().Substring(0, 35));
                    }
                    else
                    {
                        writer.WriteElementString("City", oPatient.PatientAddress.City);
                    }
                    //writer.WriteElementString("City", oPatient.PatientAddress.City);
                    writer.WriteElementString("State", oPatient.PatientAddress.State);
                    writer.WriteElementString("ZipCode", oPatient.PatientAddress.Zip);

                    //writer.WriteElementString("PlaceLocationQualifier", "");
                    writer.WriteEndElement();


                    writer.WriteEndElement(); //Patient tag ends here


                    writer.WriteStartElement("BenefitsCoordination");
                    writer.WriteStartElement("PayerIdentification");
                    writer.WriteElementString("PayerID", oPatient.RxH271Master.PayerParticipantId);
                    // oRxH271Master.PayerParticipantId)'oEligibilityCheck.Patient.Subscriber.Item(i).SubscriberID) 
                    writer.WriteEndElement(); // end PayerIdentification
                    writer.WriteElementString("PayerName", oPatient.RxH271Master.PayerName);
                    //"InsuranceCompanyName") 
                    writer.WriteElementString("CardholderID", oPatient.RxH271Master.CardHolderId);
                    // 
                    writer.WriteElementString("CardholderName", oPatient.RxH271Master.CardHolderName);
                    writer.WriteElementString("GroupID", oPatient.RxH271Master.GroupId);
                    writer.WriteElementString("EffectiveDate", DateTime.Now.AddYears(-2).ToString("yyyy-MM-dd"));

                                        //sStartDate.ToString("yyyy-MM-dd"));//,"07");
                    writer.WriteElementString("ExpirationDate", sEndDate.ToString("yyyy-MM-dd"));//,"36");
                    // System.DateTime.Now.ToString("yyyy-MM-dd")
                    writer.WriteElementString("Consent", Consent);//oPatient.RxH271Master.Consent);
                    writer.WriteElementString("PBMMemberID", oPatient.RxH271Master.MemberID);//oPatient.RxH271Master.MemberID);"B000000%111111110%002"
                    // 
                    //BenefitsCoordination 
                    writer.WriteEndElement();


                    writer.WriteEndElement();
                    //RxHistoryRequest 
                    writer.WriteEndElement();
                    //Body 
                    writer.WriteEndElement();
                    //Message 
                    writer.Close();
                    string strMessage = "";
                    if (strInfo.Length > 0)
                    {
                        if (!string.IsNullOrEmpty(strInfo.ToString().Trim()))
                        {
                            if (strInfo.ToString().EndsWith(","))
                            {

                                strMessage = strInfo.ToString().Trim().Substring(0, ((strInfo.ToString().Trim().Length) - 1));
                                strInfo = new System.Text.StringBuilder();
                                strInfo.Append(strMessage);
                            }
                        }
                    }
                    if (strInfo.ToString().Length > 0)
                    {
                        if (MessageBox.Show("Following data fields exceed number of characters allowed in x12 standards and will therefore be truncated before sending to Surescripts and PBM�s (Allowed characters are shown in parenthesis) " + System.Environment.NewLine + System.Environment.NewLine + System.Environment.NewLine + strMessage + System.Environment.NewLine + System.Environment.NewLine + System.Environment.NewLine + "Do you want to continue?", "gloEMR", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == System.Windows.Forms.DialogResult.No)
                        {
                            return;
                        }
                    }
                    strRxHisReqPath = strRxHistoryResponseFileName;

                }
                //SLR: Free settings
                if (settings != null)
                {
                    settings = null;
                }
                //assigning values to the opatient object for saving it into the RxH_HistoryMsgHeader table against genrated request 
                oPatient.MessageHeader.MessageDescription = "RxHistoryRequest";
                oPatient.MessageHeader.To = oPatient.RxH271Master.PayerParticipantId;
                oPatient.MessageHeader.From = RxHubParticipantId;//"T00000000020315"
                oPatient.MessageHeader.SecuritySecondaryID = RxHubPassword;//"FXTXGJVZ0W"
                oPatient.MessageHeader.SecurityReceiverSecID = "RxHUB";

                if (ClsgloRxHubGeneral.gblnIsRxhubStagingServer == true)//testing
                {
                    oPatient.MessageHeader.TestMessage = "1";   // "1" is for testing data else anything value is for live data
                }
                else//production
                {
                    oPatient.MessageHeader.TestMessage = "2";   // "1" is for testing data else anything value is for live data
                }
                
                oPatient.MessageHeader.SentTime = System.DateTime.Now.ToString("yyyy-MM-dd") + "T" + System.DateTime.Now.ToString("hh:mm:ss");
                oPatient.MessageHeader.MessageID = DateTime.Now.ToString("yyyyMMddhhmmss");

                //SLR: discusnnect and Free previosuly allocated medmory before assigning new one
                if (oclsgloRxHubDBLayer != null)
                {
                    oclsgloRxHubDBLayer.Disconnect();
                    oclsgloRxHubDBLayer.Dispose();
                    oclsgloRxHubDBLayer = null;
                }
                //Save data into header table
                oclsgloRxHubDBLayer = new clsgloRxHubDBLayer();
                oclsgloRxHubDBLayer.Connect(ClsgloRxHubGeneral.ConnectionString);
                oclsgloRxHubDBLayer.InsertRxH_HistoryHeader("gsp_InUpRxH_HistoryHeader", oPatient, nPatientID);
                oclsgloRxHubDBLayer.Disconnect();

                //Save data into Request detail table
                oclsgloRxHubDBLayer = new clsgloRxHubDBLayer();
                oclsgloRxHubDBLayer.Connect(ClsgloRxHubGeneral.ConnectionString);
                oclsgloRxHubDBLayer.InsertRxH_HistoryRequestDetails("gsp_InUpRxH_HistoryRequest_DTL", oPatient);
                oclsgloRxHubDBLayer.Disconnect();
            }
            catch (Exception ex)
            {
                gloAuditTrail.gloAuditTrail.ExceptionLog(gloAuditTrail.ActivityModule.Prescription, gloAuditTrail.ActivityCategory.CreatePrescription, gloAuditTrail.ActivityType.General, ex.ToString(), gloAuditTrail.ActivityOutCome.Failure);
                throw ex;
                //return null;
            }
            finally
            {
                
            }
        }
        //---------


        public bool readRxHistoryResponse(string sXmlPath, Int64 nPatientID)
        {
            bool _fResult = false;
            bool _fResponse = false;
       //     bool _fMedicationDispensed = false;
            try
            {
                ClsMedication oMedication;
                ClsSubscriber oSubscriber;
                //ClsPharmacy oPharmacy;


                // reader used to read the XML

                XmlReader oHeaderReader = default(XmlReader);
                XmlReader oHeaderSecurityReader = default(XmlReader);

                //XmlReader oNakReader = default(XmlReader);
                XmlReader oBodyReader = default(XmlReader);
                XmlReader oBodyRxHRespReader = default(XmlReader);
                XmlReader oHeaderSecurityUsernameReader = default(XmlReader);
                XmlReader oHeaderSecuritySenderReader = default(XmlReader);
                XmlReader oHeaderSecurityReceiverReader = default(XmlReader);
                XmlReader oBodyErrorReader = default(XmlReader);
                XmlReader oBodyRespDeniedReader = default(XmlReader);
                XmlReader oBodyResponseReader = default(XmlReader);
                XmlReader oBodyRespApprovedReader = default(XmlReader);
                XmlReader oBodyPharmacyReader = default(XmlReader);
                XmlReader oBodyPharmacyIdReader = default(XmlReader);
                XmlReader oBodyPharmacyPharmacistReader = default(XmlReader);
                XmlReader oBodyPharmacyAddressReader = default(XmlReader);
                XmlReader oBodyPharmacyPNoReader = default(XmlReader);
                XmlReader oBodyPharmacyPhoneNoReader = default(XmlReader);
                XmlReader oBodyPresciberReader = default(XmlReader);
                XmlReader oBodyPresciberSpecialityReader = default(XmlReader);
                XmlReader oBodyPresciberNameReader = default(XmlReader);
                XmlReader oBodyPresciberAddrReader = default(XmlReader);
                XmlReader oBodyPresciberAgentReader = default(XmlReader);
                XmlReader oBodyPatientReader = default(XmlReader);
                XmlReader oBodyPatientNameReader = default(XmlReader);
                XmlReader oBodyPatientAddrReader = default(XmlReader);
                XmlReader oBodyPatientPhNoReader = default(XmlReader);
                XmlReader oBodyPatientPhoneReader = default(XmlReader);
                XmlReader oBodyPatientPhoneNoReader = default(XmlReader);
                XmlReader oBodyBenefitsCoordinationReader = default(XmlReader);
                XmlReader oBodyBenefitsCoordPayerIdReader = default(XmlReader);
                XmlReader oBodyMedicationDispenedReader = default(XmlReader);
                XmlReader oBodyMedicationDispenedDrugCodedReader = default(XmlReader);
                XmlReader oBodyMedicationDispenedQuanReader = default(XmlReader);
                XmlReader oBodyMedicationDispenedRefillsReader = default(XmlReader);
                XmlReader oBodyMedicationDispenedDiagnosisReader = default(XmlReader);
                XmlReader oBodyMedicationDispenedDiagnosisPrimReader = default(XmlReader);
                XmlReader oBodyMedicationDispenedDiagnosisSecReader = default(XmlReader);
                XmlReader oBodyMedicationDispenedPriorAuthorizationReader = default(XmlReader);
                XmlReader oBodyMedicationDispenedPresciberPNoReader = default(XmlReader);
                XmlReader oBodyMedicationDispenedPresciberPhoneReader = default(XmlReader);
                XmlReader oBodyPharmacistReader = default(XmlReader);
                XmlReader oBodyPharmacyNCPDPIDReader = default(XmlReader);
                XmlReader oBodyMedicationDispenedDrugUseEvaluationReader = default(XmlReader);
                XmlReader oBodyMedicationDispenedDrugUseEvaluationCoAgentReader = default(XmlReader);
                XmlReader oBodyPresciberReaderPReader = default(XmlReader);
                XmlReader oBodyPresciberSpecialtyReader = default(XmlReader);
                XmlReader oBodyPresciberAMASpecialtyReader = default(XmlReader);
                XmlReader oBodyPresciberOtherSpecialityReader = default(XmlReader);
                XmlReader oBodyPresciberReaderPNoReader = default(XmlReader);
                XmlReader oBodyMedicationDispenedPharmacyReader = default(XmlReader);
                XmlReader oBodyMedicationDispenedPresciberReader = default(XmlReader);
                XmlReader oBodyMedicationDispenedPresciberAddrReader = default(XmlReader);
                XmlReader oBodyMedicationDispenedPresciberNameReader = default(XmlReader);
                XmlReader oBodyMedicationDispenedPharmacyAddrReader = default(XmlReader);
                XmlReader oBodyMedicationDispenedPharmacyPNoReader = default(XmlReader);
                XmlReader oBodyMedicationDispenedPresciberIdReader = default(XmlReader);
                XmlReader oBodyPresciberIdReader = default(XmlReader);


                XmlTextReader otextReader = default(XmlTextReader);

                //read the XML
                otextReader = new XmlTextReader(sXmlPath);
                //otextReader.Read();

                while (otextReader.Read())
                {
                    if (otextReader.NodeType == XmlNodeType.Element)
                    {
                        if (otextReader.Name == "nak")
                        {
                            //ClsgloRxHubGeneral.errorMessage = otextReader.ReadElementString("nak");
                            oPatient.MessageHeader.MessageDescription = otextReader.ReadElementString("nak");
                        }
                    }

                    switch (otextReader.Name)
                    {
                        ////Read Header Tag
                        case "Header":
                            oHeaderReader = otextReader.ReadSubtree();
                            while (oHeaderReader.Read())
                            {
                                switch (oHeaderReader.Name)
                                {
                                    case "To":
                                        oPatient.MessageHeader.To = oHeaderReader.ReadString();

                                        break;
                                    case "From":
                                        oPatient.MessageHeader.From = oHeaderReader.ReadString();
                                        break;
                                    case "MessageID":
                                        oPatient.MessageHeader.MessageID = oHeaderReader.ReadString();
                                        break;
                                    case "SentTime":
                                        oPatient.MessageHeader.SentTime = oHeaderReader.ReadString();
                                        break;
                                    case "Security":
                                        if (oHeaderReader.NodeType == XmlNodeType.Element)
                                        {
                                            oHeaderSecurityReader = oHeaderReader.ReadSubtree();
                                            while (oHeaderSecurityReader.Read())
                                            {
                                                switch (oHeaderSecurityReader.Name)
                                                {
                                                    case "UsernameToken":
                                                        if (oHeaderSecurityReader.NodeType == XmlNodeType.Element)
                                                        {
                                                            oHeaderSecurityUsernameReader = oHeaderSecurityReader.ReadSubtree();
                                                            while (oHeaderSecurityUsernameReader.Read())
                                                            {
                                                                switch (oHeaderSecurityUsernameReader.Name)
                                                                {
                                                                    case "Username":
                                                                        oPatient.MessageHeader.Security = oHeaderSecurityUsernameReader.ReadString();
                                                                        break;
                                                                }
                                                            }
                                                        }
                                                        break;
                                                    case "Sender":
                                                        if (oHeaderSecurityReader.NodeType == XmlNodeType.Element)
                                                        {
                                                            oHeaderSecuritySenderReader = oHeaderSecurityReader.ReadSubtree();
                                                            while (oHeaderSecuritySenderReader.Read())
                                                            {
                                                                switch (oHeaderSecuritySenderReader.Name)
                                                                {
                                                                    case "SecondaryIdentification":
                                                                        oPatient.MessageHeader.SecuritySecondaryID = oHeaderSecuritySenderReader.ReadString();
                                                                        break;
                                                                    case "TertiaryIdentification":
                                                                        oPatient.MessageHeader.SecurityTertiaryID = oHeaderSecuritySenderReader.ReadString();
                                                                        break;
                                                                }
                                                            }
                                                        }


                                                        break;

                                                    case "Receiver":
                                                        if (oHeaderSecurityReader.NodeType == XmlNodeType.Element)
                                                        {
                                                            oHeaderSecurityReceiverReader = oHeaderSecurityReader.ReadSubtree();
                                                            while (oHeaderSecurityReceiverReader.Read())
                                                            {
                                                                switch (oHeaderSecurityReceiverReader.Name)
                                                                {
                                                                    case "SecondaryIdentification":
                                                                        oPatient.MessageHeader.SecurityReceiverSecID = oHeaderSecurityReceiverReader.ReadString();
                                                                        break;
                                                                    case "TertiaryIdentification":
                                                                        oPatient.MessageHeader.SecurityReceiverTerID = oHeaderSecurityReceiverReader.ReadString();
                                                                        break;
                                                                }
                                                            }
                                                        }


                                                        break;
                                                }
                                            }
                                        }
                                        break;
                                    case "MailBox":
                                        oPatient.MessageHeader.MailBox = oHeaderReader.ReadString();
                                        break;
                                    case "TestMessage":
                                        oPatient.MessageHeader.TestMessage = oHeaderReader.ReadString();
                                        break;

                                }//HeaderSubTree
                            }//Header
                            break;

                        //Read Body Tag
                        case "Body":
                            oBodyReader = otextReader.ReadSubtree();
                            while (oBodyReader.Read())
                            {
                                //Main Switch Tag for Body tag
                                switch (oBodyReader.Name)
                                {
                                    //Read RxHistoryResponse Tag
                                    case "RxHistoryResponse":
                                        oPatient.MessageHeader.MessageDescription = "RxHistoryResponse";
                                        _fResponse = true;
                                        oBodyRxHRespReader = oBodyReader.ReadSubtree();
                                        while (oBodyRxHRespReader.Read())
                                        {
                                            switch (oBodyRxHRespReader.Name)
                                            {
                                                //Read RxReferenceNumber Tag
                                                case "RxReferenceNumber":

                                                    break;
                                                //End RxReferenceNumber Tag

                                                //Read PrescriberOrderNumber Tag
                                                case "PrescriberOrderNumber":
                                                    break;
                                                //End PrescriberOrderNumber Tag

                                                //Read Response Tag
                                                case "Response":
                                                    if (oBodyRxHRespReader.NodeType == XmlNodeType.Element)
                                                    {
                                                        oBodyResponseReader = oBodyRxHRespReader.ReadSubtree();
                                                        while (oBodyResponseReader.Read())
                                                        {
                                                            switch (oBodyResponseReader.Name)
                                                            {
                                                                case "Approved":

                                                                    if (oBodyResponseReader.NodeType == XmlNodeType.Element)
                                                                    {
                                                                        oBodyRespApprovedReader = oBodyResponseReader.ReadSubtree();
                                                                        while (oBodyRespApprovedReader.Read())
                                                                        {
                                                                            switch (oBodyRespApprovedReader.Name)
                                                                            {
                                                                                case "ApprovalReasonCode":
                                                                                    oPatient.Response.ApprovalReasonCode = oBodyRespApprovedReader.ReadString();

                                                                                    break;
                                                                                case "ReferenceNumber":
                                                                                    oPatient.Response.ApprovedReferenceNumber = oBodyRespApprovedReader.ReadString();
                                                                                    break;
                                                                                case "Note":
                                                                                    oPatient.Response.ApprovedNote = oBodyRespApprovedReader.ReadString();
                                                                                    break;
                                                                            }

                                                                        }
                                                                    }//End Approved
                                                                    break;
                                                                case "Denied":
                                                                    if (oBodyResponseReader.NodeType == XmlNodeType.Element)
                                                                    {
                                                                        oBodyRespDeniedReader = oBodyResponseReader.ReadSubtree();
                                                                        while (oBodyRespDeniedReader.Read())
                                                                        {
                                                                            switch (oBodyRespDeniedReader.Name)
                                                                            {
                                                                                case "DenialReasonCode":
                                                                                    oPatient.Response.DenialReasonCode = oBodyRespDeniedReader.ReadString();
                                                                                    break;
                                                                                case "ReferenceNumber":
                                                                                    oPatient.Response.DeniedReferenceNumber = oBodyRespDeniedReader.ReadString();
                                                                                    break;
                                                                                case "DenialReason":
                                                                                    oPatient.Response.DenialReason = oBodyRespDeniedReader.ReadString();
                                                                                    break;
                                                                            }
                                                                        }
                                                                    }//end Denied
                                                                    break;
                                                            }
                                                        }
                                                    } //End Response Tag
                                                    break;


                                                //read Pharmacy tag
                                                case "Pharmacy":
                                                    if (oBodyRxHRespReader.NodeType == XmlNodeType.Element)
                                                    {
                                                        oBodyPharmacyReader = oBodyRxHRespReader.ReadSubtree();

                                                        //oPharmacy = new ClsPharmacy();
                                                        while (oBodyPharmacyReader.Read())
                                                        {
                                                            switch (oBodyPharmacyReader.Name)
                                                            {
                                                                case "Identification":
                                                                    if (oBodyPharmacyReader.NodeType == XmlNodeType.Element)
                                                                    {
                                                                        oBodyPharmacyIdReader = oBodyPharmacyReader.ReadSubtree();
                                                                        // oPharmacy = new ClsPharmacy();
                                                                        while (oBodyPharmacyIdReader.Read())
                                                                        {
                                                                            switch (oBodyPharmacyIdReader.Name)
                                                                            {
                                                                                case " NCPDPID":
                                                                                    oPatient.Pharmacy.NCPDPID = Convert.ToInt64(oBodyPharmacyIdReader.ReadString());
                                                                                    break;

                                                                            }
                                                                        }

                                                                    }//End Identification Tag
                                                                    break;
                                                                case "StoreName":
                                                                    oPatient.Pharmacy.StoreName = oBodyPharmacyReader.ReadString();

                                                                    break;
                                                                case "Pharmacist":
                                                                    if (oBodyPharmacyReader.NodeType == XmlNodeType.Element)
                                                                    {
                                                                        oBodyPharmacyPharmacistReader = oBodyPharmacyReader.ReadSubtree();
                                                                        while (oBodyPharmacyPharmacistReader.Read())
                                                                        {
                                                                            switch (oBodyPharmacyPharmacistReader.Name)
                                                                            {
                                                                                case "LastName":
                                                                                    oPatient.Pharmacy.PharmacistLastName = oBodyPharmacyPharmacistReader.ReadString();
                                                                                    break;
                                                                                case "FirstName":
                                                                                    oPatient.Pharmacy.PharmacistFirstName = oBodyPharmacyPharmacistReader.ReadString();
                                                                                    break;
                                                                                case "MidddleName":
                                                                                    oPatient.Pharmacy.PharmacistMiddleName = oBodyPharmacyPharmacistReader.ReadString();
                                                                                    break;
                                                                                case "Suffix":
                                                                                    oPatient.Pharmacy.PharmacistSuffix = oBodyPharmacyPharmacistReader.ReadString();
                                                                                    break;
                                                                                case "Prefix":
                                                                                    oPatient.Pharmacy.PharmacistPrefix = oBodyPharmacyPharmacistReader.ReadString();
                                                                                    break;
                                                                            }
                                                                        }
                                                                    }
                                                                    break;

                                                                case "Address":

                                                                    if (oBodyPharmacyReader.NodeType == XmlNodeType.Element)
                                                                    {
                                                                        oBodyPharmacyAddressReader = oBodyPharmacyReader.ReadSubtree();
                                                                        while (oBodyPharmacyAddressReader.Read())
                                                                        {
                                                                            switch (oBodyPharmacyAddressReader.Name)
                                                                            {
                                                                                case "AddressLine1":
                                                                                    oPatient.Pharmacy.PhramacyAddress.AddressLine1 = oBodyPharmacyAddressReader.ReadString();
                                                                                    break;
                                                                                case "AddressLine2":
                                                                                    oPatient.Pharmacy.PhramacyAddress.AddressLine2 = oBodyPharmacyAddressReader.ReadString();
                                                                                    break;
                                                                                case "City":
                                                                                    oPatient.Pharmacy.PhramacyAddress.City = oBodyPharmacyAddressReader.ReadString();
                                                                                    break;
                                                                                case "State":
                                                                                    oPatient.Pharmacy.PhramacyAddress.State = oBodyPharmacyAddressReader.ReadString();
                                                                                    break;
                                                                                case "ZipCode":
                                                                                    oPatient.Pharmacy.PhramacyAddress.Zip = oBodyPharmacyAddressReader.ReadString();
                                                                                    break;
                                                                                case "PlaceLocationQualifier":
                                                                                    oPatient.Pharmacy.PhramacyAddress.PlaceLocationQualifier = oBodyPharmacyAddressReader.ReadString();
                                                                                    break;
                                                                            }
                                                                        }
                                                                    }

                                                                    break;
                                                                case "Email":
                                                                    oPatient.Pharmacy.PharmacyContactDetails.Email = oBodyPharmacyReader.ReadString();
                                                                    break;

                                                                case "PhoneNumbers":

                                                                    if (oBodyPharmacyReader.NodeType == XmlNodeType.Element)
                                                                    {
                                                                        oBodyPharmacyPNoReader = oBodyPharmacyReader.ReadSubtree();
                                                                        while (oBodyPharmacyPNoReader.Read())
                                                                        {
                                                                            switch (oBodyPharmacyPNoReader.Name)
                                                                            {
                                                                                case "Phone":
                                                                                    if (oBodyPharmacyPNoReader.NodeType == XmlNodeType.Element)
                                                                                    {
                                                                                        oBodyPharmacyPhoneNoReader = oBodyPharmacyPNoReader.ReadSubtree();
                                                                                        while (oBodyPharmacyPhoneNoReader.Read())
                                                                                        {
                                                                                            switch (oBodyPharmacyPhoneNoReader.Name)
                                                                                            {
                                                                                                case "Number":
                                                                                                    oPatient.Pharmacy.PharmacyContactDetails.Phone = oBodyPharmacyPhoneNoReader.ReadString();
                                                                                                    break;
                                                                                                case "Qualifier":
                                                                                                    oPatient.Pharmacy.PharmacyContactDetails.PhoneQualifier = oBodyPharmacyPhoneNoReader.ReadString(); break;
                                                                                            }
                                                                                        }
                                                                                    }

                                                                                    break;
                                                                            }
                                                                        }
                                                                    }//PhoneNumbers
                                                                    break;
                                                            }//switch

                                                        }//while
                                                    } //End Pharmacy tag

                                                    break;
                                                //Read Prescriber Tag
                                                case "Prescriber":
                                                    if (oBodyRxHRespReader.NodeType == XmlNodeType.Element)
                                                    {
                                                        oBodyPresciberReader = oBodyRxHRespReader.ReadSubtree();
                                                        while (oBodyPresciberReader.Read())
                                                        {
                                                            switch (oBodyPresciberReader.Name)
                                                            {
                                                                //Read Identification Tag
                                                                case "Identification":
                                                                    if (oBodyPresciberReader.NodeType == XmlNodeType.Element)
                                                                    {
                                                                        oBodyPresciberIdReader = oBodyPresciberReader.ReadSubtree();
                                                                        while (oBodyPresciberIdReader.Read())
                                                                        {
                                                                            switch (oBodyPresciberIdReader.Name)
                                                                            {
                                                                                //Read DEANumber Tag From Prescriber
                                                                                case "DEANumber":
                                                                                    oPatient.Provider.ProviderDEA = oBodyPresciberIdReader.ReadString();
                                                                                    break;
                                                                                case "NPI":
                                                                                    oPatient.Provider.ProviderNPI = oBodyPresciberIdReader.ReadString();
                                                                                    break;

                                                                                //End DEANumber Tag From Prescriber
                                                                            }
                                                                        }

                                                                    }//End Identification Tag
                                                                    break;

                                                                //read ClinicName Tag from Presciber Tag
                                                                case "ClinicName":
                                                                    if (oBodyPresciberReader.NodeType == XmlNodeType.Element)
                                                                    {
                                                                        oPatient.Provider.ClinicName = oBodyPresciberReader.ReadString();
                                                                    }//End CinicName Tag                                                                    
                                                                    break;

                                                                //Read Name Tag From Prescriber 
                                                                case "Name":
                                                                    if (oBodyPresciberReader.NodeType == XmlNodeType.Element)
                                                                    {
                                                                        oBodyPresciberNameReader = oBodyPresciberReader.ReadSubtree();

                                                                        while (oBodyPresciberNameReader.Read())
                                                                        {
                                                                            switch (oBodyPresciberNameReader.Name)
                                                                            {
                                                                                case "LastName":
                                                                                    oPatient.Provider.ProviderLastName = oBodyPresciberNameReader.ReadString();
                                                                                    break;
                                                                                case "FirstName":
                                                                                    oPatient.Provider.ProviderFirstName = oBodyPresciberNameReader.ReadString();
                                                                                    break;
                                                                                case "MiddleName":
                                                                                    oPatient.Provider.ProviderMiddleName = oBodyPresciberNameReader.ReadString();
                                                                                    break;
                                                                                case "Suffix":
                                                                                    oPatient.Provider.ProviderSuffix = oBodyPresciberNameReader.ReadString();
                                                                                    break;
                                                                                case "Prefix":
                                                                                    oPatient.Provider.ProviderPrefix = oBodyPresciberNameReader.ReadString();
                                                                                    break;
                                                                            }
                                                                        }
                                                                    }
                                                                    //End Name Tag From Prescriber
                                                                    break;

                                                                //read specialty tag from Prescriber
                                                                case "Spacialty":
                                                                    if (oBodyPresciberReader.NodeType == XmlNodeType.Element)
                                                                    {
                                                                        oBodyPresciberSpecialtyReader = oBodyPresciberReader.ReadSubtree();
                                                                        while (oBodyPresciberSpecialtyReader.Read())
                                                                        {
                                                                            switch (oBodyPresciberSpecialtyReader.Name)
                                                                            {
                                                                                case "AMASpecialty":
                                                                                    if (oBodyPresciberSpecialtyReader.NodeType == XmlNodeType.Element)
                                                                                    {

                                                                                        oBodyPresciberAMASpecialtyReader = oBodyPresciberSpecialityReader.ReadSubtree();
                                                                                        while (oBodyPresciberAMASpecialtyReader.Read())
                                                                                        {
                                                                                            switch (oBodyPresciberAMASpecialtyReader.Name)
                                                                                            {
                                                                                                case "SpacialtyCode":
                                                                                                    oPatient.Provider.AMASpecialtyCode = oBodyPresciberAMASpecialtyReader.ReadString();
                                                                                                    break;
                                                                                            }
                                                                                        }
                                                                                    }//   AMASpecialty                                                                                      
                                                                                    break;

                                                                                case "OtherSpecialty":
                                                                                    if (oBodyPresciberSpecialityReader.NodeType == XmlNodeType.Element)
                                                                                    {

                                                                                        oBodyPresciberOtherSpecialityReader = oBodyPresciberSpecialityReader.ReadSubtree();
                                                                                        while (oBodyPresciberOtherSpecialityReader.Read())
                                                                                        {
                                                                                            switch (oBodyPresciberOtherSpecialityReader.Name)
                                                                                            {
                                                                                                case "SpacialtyCode":
                                                                                                    oPatient.Provider.OtherSpecialtyCode = oBodyPresciberOtherSpecialityReader.ReadString();
                                                                                                    break;
                                                                                                case "Qualifier":
                                                                                                    oPatient.Provider.OtherSpecialtyQualifier = oBodyPresciberOtherSpecialityReader.ReadString();
                                                                                                    break;
                                                                                            }
                                                                                        }
                                                                                    }//OtherSpecialty
                                                                                    break;
                                                                            }
                                                                        }
                                                                    }//End specialty tag from Presciber tag
                                                                    break;


                                                                //read PresciberAgent  tag form Presciber tag

                                                                case "PresciberAgent":

                                                                    if (oBodyPresciberReader.NodeType == XmlNodeType.Element)
                                                                    {
                                                                        oBodyPresciberAgentReader = oBodyPresciberReader.ReadSubtree();
                                                                        {
                                                                            while (oBodyPresciberAgentReader.Read())
                                                                            {
                                                                                switch (oBodyPresciberAgentReader.Name)
                                                                                {
                                                                                    //read lastName Tag
                                                                                    case "LastName":
                                                                                        oPatient.Provider.PrescriberAgent.PresciberAgentLastName = oBodyPresciberAgentReader.ReadString();//ReadString();
                                                                                        break;

                                                                                    //End LastName Tag 

                                                                                    //Read FirstName Tag 
                                                                                    case "FirstName":
                                                                                        oPatient.Provider.PrescriberAgent.PresciberAgentFirstName = oBodyPresciberAgentReader.ReadString();//ReadInnerXml();
                                                                                        break;

                                                                                    //End FirstName Tag 

                                                                                    //Read MiddleName Tag 
                                                                                    case "MiddleName":
                                                                                        oPatient.Provider.PrescriberAgent.PresciberAgentMiddleName = oBodyPresciberAgentReader.ReadString();//ReadInnerXml();
                                                                                        break;
                                                                                    //End MiddleName Tag 

                                                                                    //Read Suffix Tag 
                                                                                    case "Suffix":
                                                                                        oPatient.Provider.PrescriberAgent.PresciberAgentSuffix = oBodyPresciberAgentReader.ReadString();//ReadInnerXml();
                                                                                        break;
                                                                                    //End Suffix Tag 
                                                                                    //Read Prefix Tag 
                                                                                    case "Prefix":
                                                                                        oPatient.Provider.PrescriberAgent.PresciberAgentPrefix = oBodyPresciberAgentReader.ReadString();//ReadInnerXml();
                                                                                        break;
                                                                                    //End Prefix Tag 
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                    break;
                                                                //End PresciberAgent tag

                                                                case "Address":
                                                                    if (oBodyPresciberReader.NodeType == XmlNodeType.Element)
                                                                    {
                                                                        oBodyPresciberAddrReader = oBodyPresciberReader.ReadSubtree();
                                                                        while (oBodyPresciberAddrReader.Read())
                                                                        {
                                                                            switch (oBodyPresciberAddrReader.Name)
                                                                            {

                                                                                case "AddressLine1":
                                                                                    oPatient.Provider.ProviderAddress.AddressLine1 = oBodyPresciberAddrReader.ReadString();//ReadInnerXml();
                                                                                    break;
                                                                                case "AddressLine2":
                                                                                    oPatient.Provider.ProviderAddress.AddressLine2 = oBodyPresciberAddrReader.ReadString();//ReadInnerXml();
                                                                                    break;
                                                                                case "City":
                                                                                    oPatient.Provider.ProviderAddress.City = oBodyPresciberAddrReader.ReadString();//ReadInnerXml();
                                                                                    break;
                                                                                case "State":
                                                                                    oPatient.Provider.ProviderAddress.State = oBodyPresciberAddrReader.ReadString();//ReadInnerXml();
                                                                                    break;
                                                                                case "ZipCode":
                                                                                    oPatient.Provider.ProviderAddress.Zip = oBodyPresciberAddrReader.ReadString();//ReadInnerXml();
                                                                                    break;
                                                                                case "PlaceLocationQualifier":
                                                                                    oPatient.Provider.ProviderAddress.PlaceLocationQualifier = oBodyPresciberAddrReader.ReadString();//ReadInnerXml();
                                                                                    break;
                                                                            }
                                                                        }
                                                                    }

                                                                    break;


                                                                //Read PhoneNumbers Tag from Prescriber
                                                                case "PhoneNumbers":
                                                                    if (oBodyPresciberReader.NodeType == XmlNodeType.Element)
                                                                    {
                                                                        oBodyPresciberReaderPNoReader = oBodyPresciberReader.ReadSubtree();

                                                                        while (oBodyPresciberReaderPNoReader.Read())
                                                                        {
                                                                            switch (oBodyPresciberReaderPNoReader.Name)
                                                                            {
                                                                                //Read Phone Tag from PhoneNumbers
                                                                                case "Phone":
                                                                                    if (oBodyPresciberReaderPNoReader.NodeType == XmlNodeType.Element)
                                                                                    {
                                                                                        oBodyPresciberReaderPReader = oBodyPresciberReaderPNoReader.ReadSubtree();
                                                                                        while (oBodyPresciberReaderPReader.Read())
                                                                                        {
                                                                                            switch (oBodyPresciberReaderPReader.Name)
                                                                                            {
                                                                                                //Read Number Tag from Phone
                                                                                                case "Number":
                                                                                                    oPatient.Provider.ProviderContactDtl.Phone = oBodyPresciberReaderPReader.ReadString();// ReadInnerXml();
                                                                                                    break;
                                                                                                //End Number Tag from Phone

                                                                                                //Read Qualifier Tag from PhoneNumber
                                                                                                case "Qualifier":
                                                                                                    oPatient.Provider.ProviderContactDtl.PhoneQualifier = oBodyPresciberReaderPReader.ReadString();//ReadInnerXml();
                                                                                                    break;
                                                                                                //End Qualifier Tag from PhoneNumber
                                                                                            }
                                                                                        }
                                                                                    }//End Phone Tag from PhoneNumbers
                                                                                    break;
                                                                            }
                                                                        }
                                                                    }//End PhoneNumbers Tag from Prescriber
                                                                    break;
                                                            }
                                                        }
                                                    }//End Prescriber Tag
                                                    break;

                                                //Read main Patient Tag 
                                                case "Patient":
                                                    if (oBodyRxHRespReader.NodeType == XmlNodeType.Element)
                                                    {
                                                        oBodyPatientReader = oBodyRxHRespReader.ReadSubtree();
                                                        while (oBodyPatientReader.Read())
                                                        {
                                                            switch (oBodyPatientReader.Name)
                                                            {
                                                                case "PatientRelationship":
                                                                    oPatient.RxH271Master.RelationshipCode = oBodyPatientReader.ReadString();//ReadInnerXml();
                                                                    break;
                                                                case "Identification":
                                                                    break;

                                                                //Read Name tag from Patient
                                                                case "Name":
                                                                    if (oBodyPatientReader.NodeType == XmlNodeType.Element)
                                                                    {
                                                                        oBodyPatientNameReader = oBodyPatientReader.ReadSubtree();
                                                                        while (oBodyPatientNameReader.Read())
                                                                        {
                                                                            switch (oBodyPatientNameReader.Name)
                                                                            {
                                                                                //Read LastName Tag from Patient
                                                                                case "LastName":
                                                                                    oPatient.LastName = oBodyPatientReader.ReadString();//ReadInnerXml();
                                                                                    break;
                                                                                //End LastName Tag from Patient

                                                                                //Read FirstName Tag from Patient
                                                                                case "FirstName":
                                                                                    oPatient.FirstName = oBodyPatientReader.ReadString();//ReadInnerXml();
                                                                                    break;

                                                                                //End FirstName Tag from Patient

                                                                                //Read MiddleName Tag from Patient
                                                                                case "MiddleName":
                                                                                    oPatient.MiddleName = oBodyPatientReader.ReadString();//ReadInnerXml();
                                                                                    break;

                                                                                //End MiddleName Tag from Patient

                                                                                //Read Suffix Tag from Patient
                                                                                case "Suffix":
                                                                                    oPatient.Suffix = oBodyPatientReader.ReadString();//ReadInnerXml();
                                                                                    break;

                                                                                //End Suffix Tag from Patient
                                                                                //Read Prefix Tag from Patient
                                                                                case "Prefix":
                                                                                    oPatient.Prefix = oBodyPatientReader.ReadString();//ReadInnerXml();
                                                                                    break;

                                                                                //End Prefix Tag from Patient
                                                                            }
                                                                        }
                                                                    }//End Name tag form Patient
                                                                    break;

                                                                //Read Gender Tag from Patient
                                                                case "Gender":
                                                                    oPatient.Gender = oBodyPatientReader.ReadString();//ReadInnerXml();
                                                                    break;

                                                                //End Gender Tag from Patient

                                                                //Read DateOfBirth Tag from Patient
                                                                case "DateOfBirth":
                                                                    if (oBodyPatientReader.NodeType == XmlNodeType.Element)
                                                                    {
                                                                        string sDOB = oBodyPatientReader.ReadString();
                                                                        //DateTime dtDOB = Convert.ToDateTime(oBodyPatientReader.ReadString());

                                                                        string[] arrdate = Microsoft.VisualBasic.Strings.Split(sDOB, "-", 3, Microsoft.VisualBasic.CompareMethod.Text);
                                                                        if (arrdate.Length > 0)
                                                                        {
                                                                            sDOB = arrdate[1] + "-" + arrdate[2] + "-" + arrdate[0];

                                                                        }
                                                                        oPatient.DOB = Convert.ToDateTime(sDOB);
                                                                        //oPatient.DOB = Convert.ToDateTime(sDOB);//'Convert.ToDateTime(oBodyPatientReader.ReadString());
                                                                    }
                                                                    break;

                                                                //End DateOfBirth Tag from Patient

                                                                case "Address":
                                                                    if (oBodyPatientReader.NodeType == XmlNodeType.Element)
                                                                    {
                                                                        oBodyPatientAddrReader = oBodyPatientReader.ReadSubtree();
                                                                        while (oBodyPatientAddrReader.Read())
                                                                        {
                                                                            switch (oBodyPatientAddrReader.Name)
                                                                            {
                                                                                case "AddressLine1":
                                                                                    oPatient.PatientAddress.AddressLine1 = oBodyPatientAddrReader.ReadString();//'ReadInnerXml();
                                                                                    break;
                                                                                case "AddressLine2":
                                                                                    oPatient.PatientAddress.AddressLine2 = oBodyPatientAddrReader.ReadString();//'ReadInnerXml();
                                                                                    break;
                                                                                case "City":
                                                                                    oPatient.PatientAddress.City = oBodyPatientAddrReader.ReadString();//ReadInnerXml();
                                                                                    break;
                                                                                case "State":
                                                                                    oPatient.PatientAddress.State = oBodyPatientAddrReader.ReadString();//ReadInnerXml();
                                                                                    break;
                                                                                case "ZipCode":
                                                                                    oPatient.PatientAddress.Zip = oBodyPatientAddrReader.ReadString();//ReadInnerXml();
                                                                                    break;
                                                                                case "PlaceLocationQualifier":
                                                                                    oPatient.PatientAddress.PlaceLocationQualifier = oBodyPatientAddrReader.ReadString();//'ReadInnerXml();
                                                                                    break;
                                                                            }
                                                                        }
                                                                    }

                                                                    break;

                                                                case "Email":
                                                                    oPatient.PatientContact.Email = oBodyPatientReader.ReadString();
                                                                    break;
                                                                case "PhoneNumber":
                                                                    if (oBodyPatientReader.NodeType == XmlNodeType.Element)
                                                                    {
                                                                        oBodyPatientPhNoReader = oBodyPatientReader.ReadSubtree();
                                                                        while (oBodyPatientPhNoReader.Read())
                                                                        {
                                                                            switch (oBodyPatientPhoneNoReader.Name)
                                                                            {
                                                                                case "Phone":
                                                                                    if (oBodyPatientPhoneNoReader.NodeType == XmlNodeType.Element)
                                                                                    {
                                                                                        oBodyPatientPhoneReader = oBodyPatientPhoneNoReader.ReadSubtree();
                                                                                        while (oBodyPatientPhoneReader.Read())
                                                                                        {
                                                                                            switch (oBodyPatientPhoneReader.Name)
                                                                                            {
                                                                                                case "Number":
                                                                                                    oPatient.PatientContact.Phone = oBodyPatientPhoneReader.ReadString();
                                                                                                    break;
                                                                                                case "Qualifier":
                                                                                                    oPatient.PatientContact.PhoneQualifier = oBodyPatientPhoneReader.ReadString();
                                                                                                    break;
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                    break;
                                                                            }
                                                                        }
                                                                    }
                                                                    break;
                                                            }//Switch For Patient
                                                        }
                                                    } //End Patient Tag
                                                    break;
                                                //Read BenefitsCoordination Tag 
                                                case "BenefitsCoordination":
                                                    if (oBodyRxHRespReader.NodeType == XmlNodeType.Element)
                                                    {
                                                        oBodyBenefitsCoordinationReader = oBodyRxHRespReader.ReadSubtree();
                                                        oSubscriber = new ClsSubscriber();

                                                        while (oBodyBenefitsCoordinationReader.Read())
                                                        {
                                                            switch (oBodyBenefitsCoordinationReader.Name)
                                                            {
                                                                //Read PayerIdentification Tag from BenefitsCoordination
                                                                case "PayerIdentification":
                                                                    if (oBodyBenefitsCoordinationReader.NodeType == XmlNodeType.Element)
                                                                    {
                                                                        oBodyBenefitsCoordPayerIdReader = oBodyBenefitsCoordinationReader.ReadSubtree();
                                                                        while (oBodyBenefitsCoordPayerIdReader.Read())
                                                                        {
                                                                            switch (oBodyBenefitsCoordPayerIdReader.Name)
                                                                            {
                                                                                //Read PayerID Tag from BenefitsCoordination
                                                                                case "PayerID":
                                                                                    oPatient.RxH271Master.PayerParticipantId = oBodyBenefitsCoordPayerIdReader.ReadString();
                                                                                    break;
                                                                                //End PayerID Tag from BenefitsCoordination 
                                                                            }
                                                                        }
                                                                    }
                                                                    //End PayerIdentification Tag from BenefitsCoordination
                                                                    break;
                                                                case "PayerName":
                                                                    oPatient.RxH271Master.PayerName = oBodyBenefitsCoordinationReader.ReadString();
                                                                    break;
                                                                case "CardholderID":
                                                                    oPatient.RxH271Master.CardHolderId = oBodyBenefitsCoordinationReader.ReadString();
                                                                    break;
                                                                case "CardholderName":
                                                                    oPatient.RxH271Master.CardHolderName = oBodyBenefitsCoordinationReader.ReadString();
                                                                    break;
                                                                case "GroupID":
                                                                    oPatient.RxH271Master.GroupId = oBodyBenefitsCoordinationReader.ReadString();
                                                                    break;
                                                                case "Consent":
                                                                    oPatient.RxH271Master.Consent = oBodyBenefitsCoordinationReader.ReadString();
                                                                    break;
                                                                case "PBMMemberID":
                                                                    oPatient.RxH271Master.MemberID = oBodyBenefitsCoordinationReader.ReadString();
                                                                    break;
                                                            }
                                                        }
                                                        oPatient.Subscriber.Add(oSubscriber);
                                                    }
                                                    //End BenefitsCoordination Tag
                                                    break;
                                                //Read MedicationDispensed Tag 
                                                case "MedicationDispensed":
                                                    if (oBodyRxHRespReader.NodeType == XmlNodeType.Element)
                                                    {
                                                      //  _fMedicationDispensed = true;

                                                        oBodyMedicationDispenedReader = oBodyRxHRespReader.ReadSubtree();
                                                        oMedication = new ClsMedication();


                                                        while (oBodyMedicationDispenedReader.Read())
                                                        {
                                                            switch (oBodyMedicationDispenedReader.Name)
                                                            {
                                                                //Read DrugDescription Tag MedicationDispensed
                                                                case "DrugDescription":
                                                                    oMedication.MedicationDrug.DrugName = oBodyMedicationDispenedReader.ReadString();
                                                                    break;
                                                                //End DrugDescription Tag MedicationDispensed
                                                                //Read DrugCoded Tag MedicationDispensed
                                                                case "DrugCoded":
                                                                    if (oBodyMedicationDispenedReader.NodeType == XmlNodeType.Element)
                                                                    {
                                                                        oBodyMedicationDispenedDrugCodedReader = oBodyMedicationDispenedReader.ReadSubtree();
                                                                        while (oBodyMedicationDispenedDrugCodedReader.Read())
                                                                        {
                                                                            switch (oBodyMedicationDispenedDrugCodedReader.Name)
                                                                            {
                                                                                case "ProductCode":
                                                                                    oMedication.MedicationDrug.NDCCode = oBodyMedicationDispenedDrugCodedReader.ReadString();
                                                                                    break;
                                                                                case "ProductCodeQualifier":
                                                                                    oMedication.MedicationDrug.ProductCodeQualifier = oBodyMedicationDispenedDrugCodedReader.ReadString();
                                                                                    break;

                                                                                case "DosageForm":
                                                                                    oMedication.MedicationDrug.DosageForm = oBodyMedicationDispenedDrugCodedReader.ReadString();
                                                                                    break;
                                                                                case "Strength":
                                                                                    oMedication.MedicationDrug.Strength = oBodyMedicationDispenedDrugCodedReader.ReadString();
                                                                                    break;
                                                                                case "StrengthUnits":
                                                                                    oMedication.MedicationDrug.StrengthUnits = oBodyMedicationDispenedDrugCodedReader.ReadString();
                                                                                    break;
                                                                                case "DrugDBCode":
                                                                                    oMedication.MedicationDrug.DrugDBCode = oBodyMedicationDispenedDrugCodedReader.ReadString();
                                                                                    break;
                                                                                case "DrugDBCodeQualifier":
                                                                                    oMedication.MedicationDrug.DrugDBCodeQualifier = oBodyMedicationDispenedDrugCodedReader.ReadString();
                                                                                    break;

                                                                            }
                                                                        }
                                                                    }

                                                                    break;
                                                                //End DrugCoded Tag MedicationDispensed
                                                                //Read Quantity Tag MedicationDispensed
                                                                case "Quantity":
                                                                    if (oBodyMedicationDispenedReader.NodeType == XmlNodeType.Element)
                                                                    {
                                                                        oBodyMedicationDispenedQuanReader = oBodyMedicationDispenedReader.ReadSubtree();
                                                                        while (oBodyMedicationDispenedQuanReader.Read())
                                                                        {
                                                                            switch (oBodyMedicationDispenedQuanReader.Name)
                                                                            {
                                                                                case "Qualifier":
                                                                                    oMedication.MedicationDrug.QuantityQualifier = oBodyMedicationDispenedQuanReader.ReadString();
                                                                                    break;
                                                                                case "Value":
                                                                                    oMedication.MedicationDrug.QuantityValue = oBodyMedicationDispenedQuanReader.ReadString();
                                                                                    break;
                                                                                case "CodeListQualifier":
                                                                                    oMedication.MedicationDrug.CodeListQualifier = oBodyMedicationDispenedQuanReader.ReadString();
                                                                                    break;
                                                                            }
                                                                        }
                                                                    }
                                                                    break;
                                                                //End Quantity Tag MedicationDispensed
                                                                case "DaysSupply":
                                                                    oMedication.MedicationDrug.DaysSupply = oBodyMedicationDispenedReader.ReadString();
                                                                    break;
                                                                case "Directions":
                                                                    oMedication.MedicationDrug.Directions = oBodyMedicationDispenedReader.ReadString();
                                                                    break;
                                                                case "Note":
                                                                    oMedication.MedicationDrug.Note = oBodyMedicationDispenedReader.ReadString();
                                                                    break;
                                                                case "Refills":
                                                                    if (oBodyMedicationDispenedReader.NodeType == XmlNodeType.Element)
                                                                    {
                                                                        oBodyMedicationDispenedRefillsReader = oBodyMedicationDispenedReader.ReadSubtree();
                                                                        while (oBodyMedicationDispenedRefillsReader.Read())
                                                                        {
                                                                            switch (oBodyMedicationDispenedRefillsReader.Name)
                                                                            {
                                                                                case "Qualifier":
                                                                                    oMedication.MedicationDrug.RefillsQualifier = oBodyMedicationDispenedRefillsReader.ReadString();
                                                                                    break;
                                                                                case "Quantity":
                                                                                    oMedication.MedicationDrug.RefillsQuantity = oBodyMedicationDispenedRefillsReader.ReadString();
                                                                                    break;

                                                                            }
                                                                        }
                                                                    }

                                                                    break;
                                                                case "Substitutions":
                                                                    oMedication.MedicationDrug.Substitutions = oBodyMedicationDispenedReader.ReadString();
                                                                    break;
                                                                case "WrittenDate":
                                                                    oMedication.MedicationDrug.WrittenDate = oBodyMedicationDispenedReader.ReadString();
                                                                    break;
                                                                case "LastFillDate":
                                                                    string strlastfilldate = oBodyMedicationDispenedReader.ReadString();
                                                                    string[] arrdate = Microsoft.VisualBasic.Strings.Split(strlastfilldate, "-", 3, Microsoft.VisualBasic.CompareMethod.Text);
                                                                    if (arrdate.Length > 0)
                                                                    {
                                                                        strlastfilldate = arrdate[1] + "-" + arrdate[2] + "-" + arrdate[0];

                                                                    }
                                                                    oMedication.MedicationDrug.LastFillDate = Convert.ToDateTime(strlastfilldate);
                                                                    //oMedication.MedicationDrug.LastFillDate = Convert.ToDateTime(oBodyMedicationDispenedReader.ReadString().ToString("MM/dd/yyyy"));
                                                                    break;
                                                                case "ExpirationDate":
                                                                    oMedication.MedicationDrug.ExpirationDate = oBodyMedicationDispenedReader.ReadString();
                                                                    break;
                                                                case "EffectiveDate":
                                                                    oMedication.MedicationDrug.EffectiveDate = oBodyMedicationDispenedReader.ReadString();
                                                                    break;
                                                                case "PeriodEnd":
                                                                    oMedication.MedicationDrug.PeriodEnd = oBodyMedicationDispenedReader.ReadString();
                                                                    break;
                                                                case "Diagnosis":

                                                                    if (oBodyMedicationDispenedReader.NodeType == XmlNodeType.Element)
                                                                    {
                                                                        oBodyMedicationDispenedDiagnosisReader = oBodyMedicationDispenedReader.ReadSubtree();
                                                                        while (oBodyMedicationDispenedDiagnosisReader.Read())
                                                                        {
                                                                            switch (oBodyMedicationDispenedDiagnosisReader.Name)
                                                                            {
                                                                                case "ClinicalInformationQualifier":
                                                                                    oMedication.MedicationDrug.DiagnosisClinicalInformation = oBodyMedicationDispenedDiagnosisReader.ReadString();
                                                                                    break;
                                                                                case "Primary":
                                                                                    if (oBodyMedicationDispenedDiagnosisReader.NodeType == XmlNodeType.Element)
                                                                                    {
                                                                                        oBodyMedicationDispenedDiagnosisPrimReader = oBodyMedicationDispenedDiagnosisReader.ReadSubtree();
                                                                                        while (oBodyMedicationDispenedDiagnosisPrimReader.Read())
                                                                                        {
                                                                                            switch (oBodyMedicationDispenedDiagnosisPrimReader.Name)
                                                                                            {
                                                                                                case "Qualifier":
                                                                                                    oMedication.MedicationDrug.DiagnosisPriamryQualifier = oBodyMedicationDispenedDiagnosisPrimReader.ReadString();
                                                                                                    break;
                                                                                                case "Value":
                                                                                                    oMedication.MedicationDrug.DiagnosisPriamryValue = oBodyMedicationDispenedDiagnosisPrimReader.ReadString();
                                                                                                    break;

                                                                                            }
                                                                                        }
                                                                                    }

                                                                                    break;
                                                                                case "Secondary":
                                                                                    if (oBodyMedicationDispenedDiagnosisReader.NodeType == XmlNodeType.Element)
                                                                                    {
                                                                                        oBodyMedicationDispenedDiagnosisSecReader = oBodyMedicationDispenedDiagnosisReader.ReadSubtree();
                                                                                        while (oBodyMedicationDispenedDiagnosisSecReader.Read())
                                                                                        {
                                                                                            switch (oBodyMedicationDispenedDiagnosisSecReader.Name)
                                                                                            {
                                                                                                case "Qualifier":
                                                                                                    oMedication.MedicationDrug.DiagnosisSecQualifier = oBodyMedicationDispenedDiagnosisSecReader.ReadString();
                                                                                                    break;
                                                                                                case "Value":
                                                                                                    oMedication.MedicationDrug.DiagnosisSecValue = oBodyMedicationDispenedDiagnosisSecReader.ReadString();
                                                                                                    break;

                                                                                            }
                                                                                        }
                                                                                    }

                                                                                    break;

                                                                            }
                                                                        }
                                                                    }
                                                                    break;
                                                                case "PriorAuthorization":
                                                                    if (oBodyMedicationDispenedReader.NodeType == XmlNodeType.Element)
                                                                    {
                                                                        oBodyMedicationDispenedPriorAuthorizationReader = oBodyMedicationDispenedReader.ReadSubtree();
                                                                        while (oBodyMedicationDispenedPriorAuthorizationReader.Read())
                                                                        {
                                                                            switch (oBodyMedicationDispenedPriorAuthorizationReader.Name)
                                                                            {
                                                                                case "Qualifier":
                                                                                    oMedication.MedicationDrug.PriorAuthorizationQualifier = oBodyMedicationDispenedPriorAuthorizationReader.ReadString();
                                                                                    break;
                                                                                case "Value":
                                                                                    oMedication.MedicationDrug.PriorAuthorizationValue = oBodyMedicationDispenedPriorAuthorizationReader.ReadString();
                                                                                    break;

                                                                            }
                                                                        }
                                                                    }

                                                                    break;

                                                                case "Pharmacy":
                                                                    if (oBodyMedicationDispenedReader.NodeType == XmlNodeType.Element)
                                                                    {
                                                                        oBodyMedicationDispenedPharmacyReader = oBodyMedicationDispenedReader.ReadSubtree();
                                                                        while (oBodyMedicationDispenedPharmacyReader.Read())
                                                                        {
                                                                            switch (oBodyMedicationDispenedPharmacyReader.Name)
                                                                            {
                                                                                case "Identification":
                                                                                    if (oBodyMedicationDispenedPharmacyReader.NodeType == XmlNodeType.Element)
                                                                                    {
                                                                                        oBodyPharmacyNCPDPIDReader = oBodyMedicationDispenedPharmacyReader.ReadSubtree();
                                                                                        while (oBodyPharmacyNCPDPIDReader.Read())
                                                                                        {
                                                                                            switch (oBodyPharmacyNCPDPIDReader.Name)
                                                                                            {
                                                                                                case "NCPDPID":
                                                                                                    //string xyz = oBodyMedicationDispenedPharmacyReader.ReadString();
                                                                                                    oMedication.MedicationPharmacy.NCPDPID = Int64.Parse(oBodyMedicationDispenedPharmacyReader.ReadString());
                                                                                                    break;
                                                                                            }
                                                                                        }

                                                                                    }
                                                                                    break;
                                                                                case "StoreName":
                                                                                    oMedication.MedicationPharmacy.StoreName = oBodyMedicationDispenedPharmacyReader.ReadString();

                                                                                    break;
                                                                                case "Pharmacist":

                                                                                    if (oBodyMedicationDispenedPharmacyReader.NodeType == XmlNodeType.Element)
                                                                                    {
                                                                                        oBodyPharmacistReader = oBodyMedicationDispenedPharmacyReader.ReadSubtree();

                                                                                        while (oBodyPharmacistReader.Read())
                                                                                        {
                                                                                            switch (oBodyPharmacistReader.Name)
                                                                                            {
                                                                                                case "LastName":
                                                                                                    oMedication.MedicationPharmacy.PharmacistLastName = oBodyPharmacistReader.ReadString();
                                                                                                    break;
                                                                                                case "FirstName":
                                                                                                    oMedication.MedicationPharmacy.PharmacistFirstName = oBodyPharmacistReader.ReadString();
                                                                                                    break;
                                                                                                case "MiddleName":
                                                                                                    oMedication.MedicationPharmacy.PharmacistMiddleName = oBodyPharmacistReader.ReadString();
                                                                                                    break;
                                                                                                case "Suffix":
                                                                                                    oMedication.MedicationPharmacy.PharmacistSuffix = oBodyPharmacistReader.ReadString();
                                                                                                    break;
                                                                                                case "Prefix":
                                                                                                    oMedication.MedicationPharmacy.PharmacistPrefix = oBodyPharmacistReader.ReadString();
                                                                                                    break;
                                                                                            }
                                                                                        }
                                                                                    }

                                                                                    break;
                                                                                case "Address":
                                                                                    if (oBodyMedicationDispenedPharmacyReader.NodeType == XmlNodeType.Element)
                                                                                    {
                                                                                        oBodyMedicationDispenedPharmacyAddrReader = oBodyMedicationDispenedPharmacyReader.ReadSubtree();
                                                                                        while (oBodyMedicationDispenedPharmacyAddrReader.Read())
                                                                                        {
                                                                                            switch (oBodyMedicationDispenedPharmacyAddrReader.Name)
                                                                                            {
                                                                                                case "AddressLine1":
                                                                                                    oMedication.MedicationPharmacy.PhramacyAddress.AddressLine1 = oBodyMedicationDispenedPharmacyAddrReader.ReadString();
                                                                                                    break;
                                                                                                case "AddressLine2":
                                                                                                    oMedication.MedicationPharmacy.PhramacyAddress.AddressLine2 = oBodyMedicationDispenedPharmacyAddrReader.ReadString();
                                                                                                    break;
                                                                                                case "City":
                                                                                                    oMedication.MedicationPharmacy.PhramacyAddress.City = oBodyMedicationDispenedPharmacyAddrReader.ReadString();
                                                                                                    break;
                                                                                                case "State":
                                                                                                    oMedication.MedicationPharmacy.PhramacyAddress.State = oBodyMedicationDispenedPharmacyAddrReader.ReadString();
                                                                                                    break;
                                                                                                case "ZipCode":
                                                                                                    oMedication.MedicationPharmacy.PhramacyAddress.Zip = oBodyMedicationDispenedPharmacyAddrReader.ReadString();
                                                                                                    break;
                                                                                                case "PlaceLocationQualifier":
                                                                                                    oMedication.MedicationPharmacy.PhramacyAddress.PlaceLocationQualifier = oBodyMedicationDispenedPharmacyAddrReader.ReadString();
                                                                                                    break;
                                                                                            }
                                                                                        }
                                                                                    }

                                                                                    break;
                                                                                case "Email":
                                                                                    oMedication.MedicationPharmacy.PharmacyContactDetails.Email = oBodyMedicationDispenedPharmacyReader.ReadString();
                                                                                    break;

                                                                                case "PhoneNumbers":
                                                                                    if (oBodyMedicationDispenedPharmacyReader.NodeType == XmlNodeType.Element)
                                                                                    {
                                                                                        oBodyMedicationDispenedPharmacyPNoReader = oBodyMedicationDispenedPharmacyReader.ReadSubtree();
                                                                                        while (oBodyMedicationDispenedPharmacyPNoReader.Read())
                                                                                        {
                                                                                            switch (oBodyMedicationDispenedPharmacyPNoReader.Name)
                                                                                            {
                                                                                                case "Phone":
                                                                                                    if (oBodyMedicationDispenedPharmacyPNoReader.NodeType == XmlNodeType.Element)
                                                                                                    {
                                                                                                        oBodyMedicationDispenedPharmacyPNoReader = oBodyMedicationDispenedPharmacyPNoReader.ReadSubtree();
                                                                                                        while (oBodyMedicationDispenedPharmacyPNoReader.Read())
                                                                                                        {
                                                                                                            switch (oBodyMedicationDispenedPharmacyPNoReader.Name)
                                                                                                            {
                                                                                                                case "Number":
                                                                                                                    oMedication.MedicationPharmacy.PharmacyContactDetails.Phone = oBodyMedicationDispenedPharmacyPNoReader.ReadString();
                                                                                                                    break;
                                                                                                                case "Qualifier":
                                                                                                                    oMedication.MedicationPharmacy.PharmacyContactDetails.PhoneQualifier = oBodyMedicationDispenedPharmacyPNoReader.ReadString();

                                                                                                                    break;
                                                                                                            }
                                                                                                        }
                                                                                                    }

                                                                                                    break;
                                                                                            }
                                                                                        }
                                                                                    }


                                                                                    break;
                                                                            }
                                                                        }

                                                                    }

                                                                    break;
                                                                case "Prescriber":
                                                                    if (oBodyMedicationDispenedReader.NodeType == XmlNodeType.Element)
                                                                    {
                                                                        oBodyMedicationDispenedPresciberReader = oBodyMedicationDispenedReader.ReadSubtree();
                                                                        while (oBodyMedicationDispenedPresciberReader.Read())
                                                                        {
                                                                            switch (oBodyMedicationDispenedPresciberReader.Name)
                                                                            {
                                                                                case "Identification":
                                                                                    if (oBodyMedicationDispenedPresciberReader.NodeType == XmlNodeType.Element)
                                                                                    {
                                                                                        oBodyMedicationDispenedPresciberIdReader = oBodyMedicationDispenedPresciberReader.ReadSubtree();
                                                                                        while (oBodyMedicationDispenedPresciberIdReader.Read())
                                                                                        {
                                                                                            switch (oBodyMedicationDispenedPresciberIdReader.Name)
                                                                                            {
                                                                                                case "DEANumber":
                                                                                                    oMedication.MedicationDrug.DrugProvider.ProviderDEA = oBodyMedicationDispenedPresciberIdReader.ReadString();
                                                                                                    break;
                                                                                                case "NPI":
                                                                                                    oMedication.MedicationDrug.DrugProvider.ProviderNPI = oBodyMedicationDispenedPresciberIdReader.ReadString();
                                                                                                    break;
                                                                                            }
                                                                                        }

                                                                                    }
                                                                                    break;
                                                                                case "ClinicName":
                                                                                    oMedication.MedicationDrug.DrugProvider.ClinicName = oBodyMedicationDispenedPresciberReader.ReadString();
                                                                                    break;

                                                                                case "Name":
                                                                                    if (oBodyMedicationDispenedPresciberReader.NodeType == XmlNodeType.Element)
                                                                                    {
                                                                                        oBodyMedicationDispenedPresciberNameReader = oBodyMedicationDispenedPresciberReader.ReadSubtree();
                                                                                        while (oBodyMedicationDispenedPresciberNameReader.Read())
                                                                                        {
                                                                                            switch (oBodyMedicationDispenedPresciberNameReader.Name)
                                                                                            {
                                                                                                case "LastName":
                                                                                                    oMedication.MedicationDrug.DrugProvider.ProviderLastName = oBodyMedicationDispenedPresciberReader.ReadString();
                                                                                                    break;
                                                                                                case "FirstName":
                                                                                                    oMedication.MedicationDrug.DrugProvider.ProviderFirstName = oBodyMedicationDispenedPresciberReader.ReadString();
                                                                                                    break;
                                                                                                case "MiddleName":
                                                                                                    oMedication.MedicationDrug.DrugProvider.ProviderMiddleName = oBodyMedicationDispenedPresciberReader.ReadString();
                                                                                                    break;
                                                                                                case "Suffix":
                                                                                                    oMedication.MedicationDrug.DrugProvider.ProviderSuffix = oBodyMedicationDispenedPresciberReader.ReadString();
                                                                                                    break;
                                                                                                case "Prefix":
                                                                                                    oMedication.MedicationDrug.DrugProvider.ProviderPrefix = oBodyMedicationDispenedPresciberReader.ReadString();
                                                                                                    break;
                                                                                            }
                                                                                        }
                                                                                    }

                                                                                    break;
                                                                                case "Address":
                                                                                    if (oBodyMedicationDispenedPresciberReader.NodeType == XmlNodeType.Element)
                                                                                    {
                                                                                        oBodyMedicationDispenedPresciberAddrReader = oBodyMedicationDispenedPresciberReader.ReadSubtree();
                                                                                        while (oBodyMedicationDispenedPresciberAddrReader.Read())
                                                                                        {
                                                                                            switch (oBodyMedicationDispenedPresciberAddrReader.Name)
                                                                                            {
                                                                                                case "AddressLine1":
                                                                                                    oMedication.MedicationDrug.DrugProvider.ProviderAddress.AddressLine1 = oBodyMedicationDispenedPresciberAddrReader.ReadString();
                                                                                                    break;
                                                                                                case "AddressLine2":
                                                                                                    oMedication.MedicationDrug.DrugProvider.ProviderAddress.AddressLine2 = oBodyMedicationDispenedPresciberAddrReader.ReadString();
                                                                                                    break;
                                                                                                case "City":
                                                                                                    oMedication.MedicationDrug.DrugProvider.ProviderAddress.City = oBodyMedicationDispenedPresciberAddrReader.ReadString();
                                                                                                    break;
                                                                                                case "State":
                                                                                                    oMedication.MedicationDrug.DrugProvider.ProviderAddress.State = oBodyMedicationDispenedPresciberAddrReader.ReadString();
                                                                                                    break;
                                                                                                case "ZipCode":
                                                                                                    oMedication.MedicationDrug.DrugProvider.ProviderAddress.Zip = oBodyMedicationDispenedPresciberAddrReader.ReadString();
                                                                                                    break;
                                                                                                case "PlaceLocationQualifier":
                                                                                                    oMedication.MedicationDrug.DrugProvider.ProviderAddress.PlaceLocationQualifier = oBodyMedicationDispenedPresciberAddrReader.ReadString();
                                                                                                    break;
                                                                                            }
                                                                                        }
                                                                                    }

                                                                                    break;

                                                                                case "Email":
                                                                                    oMedication.MedicationDrug.DrugProvider.ProviderContactDtl.Email = oBodyMedicationDispenedPresciberReader.ReadString();
                                                                                    break;
                                                                                case "PhoneNumbers":
                                                                                    if (oBodyMedicationDispenedPresciberReader.NodeType == XmlNodeType.Element)
                                                                                    {
                                                                                        oBodyMedicationDispenedPresciberPNoReader = oBodyMedicationDispenedPresciberReader.ReadSubtree();
                                                                                        while (oBodyMedicationDispenedPresciberPNoReader.Read())
                                                                                        {
                                                                                            switch (oBodyMedicationDispenedPresciberPNoReader.Name)
                                                                                            {
                                                                                                case "Phone":
                                                                                                    if (oBodyMedicationDispenedPresciberPNoReader.NodeType == XmlNodeType.Element)
                                                                                                    {
                                                                                                        oBodyMedicationDispenedPresciberPhoneReader = oBodyMedicationDispenedPresciberPNoReader.ReadSubtree();
                                                                                                        while (oBodyMedicationDispenedPresciberPhoneReader.Read())
                                                                                                        {
                                                                                                            switch (oBodyMedicationDispenedPresciberPhoneReader.Name)
                                                                                                            {
                                                                                                                case "Number":
                                                                                                                    oMedication.MedicationDrug.DrugProvider.ProviderContactDtl.Phone = oBodyMedicationDispenedPresciberPhoneReader.ReadString();
                                                                                                                    break;
                                                                                                                case "Qualifier":
                                                                                                                    oMedication.MedicationDrug.DrugProvider.ProviderContactDtl.PhoneQualifier = oBodyMedicationDispenedPresciberPhoneReader.ReadString();
                                                                                                                    break;
                                                                                                            }
                                                                                                        }
                                                                                                    }

                                                                                                    break;

                                                                                            }
                                                                                        }
                                                                                    }
                                                                                    break;

                                                                            }
                                                                        }
                                                                    }//presciber

                                                                    break;
                                                                case "DrugUseEvaluation":

                                                                    if (oBodyMedicationDispenedPresciberReader.NodeType == XmlNodeType.Element)
                                                                    {
                                                                        oBodyMedicationDispenedDrugUseEvaluationReader = oBodyMedicationDispenedPresciberReader.ReadSubtree();
                                                                        while (oBodyMedicationDispenedDrugUseEvaluationReader.Read())
                                                                        {
                                                                            switch (oBodyMedicationDispenedDrugUseEvaluationReader.Name)
                                                                            {
                                                                                case "ServiceReasonCode":

                                                                                    break;
                                                                                case "ProfessionalServiceCode":
                                                                                    break;
                                                                                case "ServiceResultCode":
                                                                                    break;
                                                                                case "CoAgent":

                                                                                    if (oBodyMedicationDispenedDrugUseEvaluationReader.NodeType == XmlNodeType.Element)
                                                                                    {
                                                                                        oBodyMedicationDispenedDrugUseEvaluationCoAgentReader = oBodyMedicationDispenedDrugUseEvaluationReader.ReadSubtree();
                                                                                        while (oBodyMedicationDispenedDrugUseEvaluationCoAgentReader.Read())
                                                                                        {
                                                                                            switch (oBodyMedicationDispenedDrugUseEvaluationCoAgentReader.Name)
                                                                                            {
                                                                                                case "CoAgentID":
                                                                                                    // oMedication.MedicationPharmacy.PharmacyContactDetails.Phone = oBodyMedicationDispenedDrugUseEvaluationCoAgentReader.ReadString();
                                                                                                    break;
                                                                                                case "CoAgentQualifier":
                                                                                                    //  oMedication.MedicationPharmacy.PharmacyContactDetails.PhoneQualifier=oBodyMedicationDispenedDrugUseEvaluationCoAgentReader.ReadString();
                                                                                                    break;
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                    break;

                                                                            }
                                                                        }
                                                                    }


                                                                    break;
                                                                case "DrugCoverageStatusCode":

                                                                    break;

                                                                // 
                                                            }
                                                        }
                                                        //add to collection
                                                        oPatient.Medication.Add(oMedication);

                                                    }//End MedicationDispensed Tag 

                                                    break;
                                            }//End Switch Tag
                                        }//End RxHistoryResponse

                                        break;                                    
                                    //added here
                                    case "Error":

                                        if (oBodyReader.NodeType == XmlNodeType.Element)
                                        {

                                            if (oBodyErrorReader != null)
                                            {
                                                oBodyErrorReader.Close();
                                                oBodyErrorReader = null;
                                            }
                                            oBodyErrorReader = oBodyReader.ReadSubtree();

                                            string _errorDesc = "";
                                            while (oBodyErrorReader.Read())
                                            {

                                                switch (oBodyErrorReader.Name)
                                                {
                                                    case "Code":

                                                        _errorDesc = oBodyErrorReader.ReadString().ToString();

                                                        break;

                                                    //case "DescriptionCode":
                                                    //    //  oMedication.MedicationPharmacy.PharmacyContactDetails.PhoneQualifier=oBodyMedicationDispenedDrugUseEvaluationCoAgentReader.ReadString();
                                                    //    break;
                                                    case "Description":
                                                        string _tempDesc = "";

                                                        _tempDesc = oBodyErrorReader.ReadString();//.ReadContentAsString().ToString();

                                                        if (_tempDesc.ToString() == "")  // desc blank.. now show custom message
                                                        {
                                                            //string _customMessage = "";
                                                            switch (_errorDesc)
                                                            {
                                                                case "602":
                                                                    _errorDesc = "602 � There has been a receiver system error while sending your request.";

                                                                    break;
                                                                case "601":

                                                                    _errorDesc = "601 � gloEMR is unable to process the message received";
                                                                    break;
                                                                case "600":

                                                                    _errorDesc = "600 - There was a communication problem please try again later";
                                                                    break;
                                                                case "900":
                                                                    _errorDesc = "900 - The transaction you sent has been rejected by the PBM�s";
                                                                    break;
                                                            }

                                                        }
                                                        else
                                                        {
                                                            _errorDesc += " - " + _tempDesc;
                                                        }
                                                        oPatient.MessageHeader.MessageDescription = _errorDesc;
                                                        oBodyErrorReader.Close();
                                                        break;
                                                }
                                            }
                                        }//error
                                        break;
                                    //---------

                                }
                            }//End Body Tag

                            break;
                    }//Main switch
                }//While

                
                //set error msg if we got the error in response
                ClsgloRxHubGeneral.errorMessage = oPatient.MessageHeader.MessageDescription;

                //set Denied reason if we get
                ClsgloRxHubGeneral.DeniedReason = oPatient.Response.DenialReason;

                //check the flag for "more drug history available"
                if (oPatient.Response.DenialReasonCode == "AQ" || oPatient.Response.ApprovalReasonCode == "AQ")
                {
                    ClsgloRxHubGeneral.moreDrugAvailableFlag = true;
                }
                else
                {
                    ClsgloRxHubGeneral.moreDrugAvailableFlag = false;
                }

                //Check the messagedescription type for saving data according to it's type
                if (oPatient.MessageHeader.MessageDescription == "RxHistoryResponse")
                {
                    //SLR: Disconncet, frree previousul allocated memory before allocating once more
                    if (oclsgloRxHubDBLayer != null)
                    {
                        oclsgloRxHubDBLayer.Disconnect();
                        oclsgloRxHubDBLayer.Dispose();
                        oclsgloRxHubDBLayer = null;
                    }
                    //Save data into header table
                    oclsgloRxHubDBLayer = new clsgloRxHubDBLayer();
                    oclsgloRxHubDBLayer.Connect(ClsgloRxHubGeneral.ConnectionString);
                    oclsgloRxHubDBLayer.InsertRxH_HistoryHeader("gsp_InUpRxH_HistoryHeader", oPatient, nPatientID);
                    oclsgloRxHubDBLayer.Disconnect();

                    //if (ClsgloRxHubGeneral.moreDrugAvailableFlag == false)
                    //{
                    //SLR: Disconncet, frree previousul allocated memory before allocating once more
                    if (oclsgloRxHubDBLayer != null)
                    {
                        oclsgloRxHubDBLayer.Disconnect();
                        oclsgloRxHubDBLayer.Dispose();
                        oclsgloRxHubDBLayer = null;
                    }
                    //Delete Rx History Response Details 
                    oclsgloRxHubDBLayer = new clsgloRxHubDBLayer();
                    oclsgloRxHubDBLayer.Connect(ClsgloRxHubGeneral.ConnectionString);
                    oclsgloRxHubDBLayer.Delete_RxH_HistoryRespDetails(nPatientID);
                    oclsgloRxHubDBLayer.Disconnect();
                    //}

                    //SLR: Disconncet, frree previousul allocated memory before allocating once more
                    if (oclsgloRxHubDBLayer != null)
                    {
                        oclsgloRxHubDBLayer.Disconnect();
                        oclsgloRxHubDBLayer.Dispose();
                        oclsgloRxHubDBLayer = null;
                    }
                    //Save data into Response detail table
                    oclsgloRxHubDBLayer = new clsgloRxHubDBLayer();
                    oclsgloRxHubDBLayer.Connect(ClsgloRxHubGeneral.ConnectionString);
                    oclsgloRxHubDBLayer.InsertRxH_HistoryRespDetails("gsp_InUpRxH_HistoryRespDetails", oPatient, nPatientID);
                    oclsgloRxHubDBLayer.Disconnect();

                }
                else
                {
                    //SLR: Disconncet, frree previousul allocated memory before allocating once more
                    if (oclsgloRxHubDBLayer != null)
                    {
                        oclsgloRxHubDBLayer.Disconnect();
                        oclsgloRxHubDBLayer.Dispose();
                        oclsgloRxHubDBLayer = null;
                    }
                    oclsgloRxHubDBLayer = new clsgloRxHubDBLayer();
                    oclsgloRxHubDBLayer.Connect(ClsgloRxHubGeneral.ConnectionString);
                    oclsgloRxHubDBLayer.InsertRxH_HistoryHeader("gsp_InUpRxH_HistoryHeader", oPatient, nPatientID);
                    oclsgloRxHubDBLayer.Disconnect();

                }
                if (_fResponse == true)
                {
                    if (oPatient.Response.DenialReason.ToString() != "")
                        _fResult = false;
                    else
                        _fResult = true;

                    //_fResult = true;
                }

                if (oBodyReader != null)
                {
                    oBodyReader.Close();
                    oBodyReader = null;
                }
                if (oBodyRxHRespReader != null)
                {
                    oBodyRxHRespReader.Close();
                    oBodyRxHRespReader = null;
                }
                if (oHeaderSecurityUsernameReader != null)
                {
                    oHeaderSecurityUsernameReader.Close();
                    oHeaderSecurityUsernameReader = null;
                }
                if (oHeaderSecuritySenderReader != null)
                {
                    oHeaderSecuritySenderReader.Close();
                    oHeaderSecuritySenderReader = null;
                }
                if (oHeaderSecurityReceiverReader != null)
                {
                    oHeaderSecurityReceiverReader.Close();
                    oHeaderSecurityReceiverReader = null;
                }
                if (oBodyErrorReader != null)
                {
                    oBodyErrorReader.Close();
                    oBodyErrorReader = null;
                }
                if (oBodyRespDeniedReader != null)
                {
                    oBodyRespDeniedReader.Close();
                    oBodyRespDeniedReader = null;
                }
                if (oBodyResponseReader != null)
                {
                    oBodyResponseReader.Close();
                    oBodyResponseReader = null;
                }
                if (oBodyRespApprovedReader != null)
                {
                    oBodyRespApprovedReader.Close();
                    oBodyRespApprovedReader = null;
                }
                if (oBodyPharmacyReader != null)
                {
                    oBodyPharmacyReader.Close();
                    oBodyPharmacyReader = null;
                }
                if (oBodyPharmacyIdReader != null)
                {
                    oBodyPharmacyIdReader.Close();
                    oBodyPharmacyIdReader = null;
                }
                if (oBodyPharmacyPharmacistReader != null)
                {
                    oBodyPharmacyPharmacistReader.Close();
                    oBodyPharmacyPharmacistReader = null;
                }
                if (oBodyPharmacyAddressReader != null)
                {
                    oBodyPharmacyAddressReader.Close();
                    oBodyPharmacyAddressReader = null;
                }
                if (oBodyPharmacyPNoReader != null)
                {
                    oBodyPharmacyPNoReader.Close();
                    oBodyPharmacyPNoReader = null;
                }
                if (oBodyPharmacyPhoneNoReader != null)
                {
                    oBodyPharmacyPhoneNoReader.Close();
                    oBodyPharmacyPhoneNoReader = null;
                }
                if (oBodyPresciberReader != null)
                {
                    oBodyPresciberReader.Close();
                    oBodyPresciberReader = null;
                }
                if (oBodyPresciberSpecialityReader != null)
                {
                    oBodyPresciberSpecialityReader.Close();
                    oBodyPresciberSpecialityReader = null;
                }
                if (oBodyPresciberNameReader != null)
                {
                    oBodyPresciberNameReader.Close();
                    oBodyPresciberNameReader = null;
                }
                if (oBodyPresciberAddrReader != null)
                {
                    oBodyPresciberAddrReader.Close();
                    oBodyPresciberAddrReader = null;
                }
                if (oBodyPresciberAgentReader != null)
                {
                    oBodyPresciberAgentReader.Close();
                    oBodyPresciberAgentReader = null;
                }
                if (oBodyPatientReader != null)
                {
                    oBodyPatientReader.Close();
                    oBodyPatientReader = null;
                }
                if (oBodyPatientNameReader != null)
                {
                    oBodyPatientNameReader.Close();
                    oBodyPatientNameReader = null;
                }
                if (oBodyPatientAddrReader != null)
                {
                    oBodyPatientAddrReader.Close();
                    oBodyPatientAddrReader = null;
                }
                //''''''''
                if (oBodyPatientPhNoReader != null)
                {
                    oBodyPatientPhNoReader.Close();
                    oBodyPatientPhNoReader = null;
                }
                if (oBodyPatientPhoneReader != null)
                {
                    oBodyPatientPhoneReader.Close();
                    oBodyPatientPhoneReader = null;
                }
                if (oBodyPatientPhoneNoReader != null)
                {
                    oBodyPatientPhoneNoReader.Close();
                    oBodyPatientPhoneNoReader = null;
                }
                if (oBodyBenefitsCoordinationReader != null)
                {
                    oBodyBenefitsCoordinationReader.Close();
                    oBodyBenefitsCoordinationReader = null;
                }
                if (oBodyBenefitsCoordPayerIdReader != null)
                {
                    oBodyBenefitsCoordPayerIdReader.Close();
                    oBodyBenefitsCoordPayerIdReader = null;
                }
                if (oBodyMedicationDispenedReader != null)
                {
                    oBodyMedicationDispenedReader.Close();
                    oBodyMedicationDispenedReader = null;
                }
                if (oBodyMedicationDispenedDrugCodedReader != null)
                {
                    oBodyMedicationDispenedDrugCodedReader.Close();
                    oBodyMedicationDispenedDrugCodedReader = null;
                }
                if (oBodyMedicationDispenedQuanReader != null)
                {
                    oBodyMedicationDispenedQuanReader.Close();
                    oBodyMedicationDispenedQuanReader = null;
                }
                if (oBodyMedicationDispenedRefillsReader != null)
                {
                    oBodyMedicationDispenedRefillsReader.Close();
                    oBodyMedicationDispenedRefillsReader = null;
                }
                if (oBodyMedicationDispenedDiagnosisReader != null)
                {
                    oBodyMedicationDispenedDiagnosisReader.Close();
                    oBodyMedicationDispenedDiagnosisReader = null;
                }
                if (oBodyMedicationDispenedDiagnosisPrimReader != null)
                {
                    oBodyMedicationDispenedDiagnosisPrimReader.Close();
                    oBodyMedicationDispenedDiagnosisPrimReader = null;
                }
                if (oBodyMedicationDispenedDiagnosisSecReader != null)
                {
                    oBodyMedicationDispenedDiagnosisSecReader.Close();
                    oBodyMedicationDispenedDiagnosisSecReader = null;
                }
                if (oBodyMedicationDispenedPriorAuthorizationReader != null)
                {
                    oBodyMedicationDispenedPriorAuthorizationReader.Close();
                    oBodyMedicationDispenedPriorAuthorizationReader = null;
                }
                if (oBodyMedicationDispenedPresciberPNoReader != null)
                {
                    oBodyMedicationDispenedPresciberPNoReader.Close();
                    oBodyMedicationDispenedPresciberPNoReader = null;
                }
                if (oBodyMedicationDispenedPresciberPhoneReader != null)
                {
                    oBodyMedicationDispenedPresciberPhoneReader.Close();
                    oBodyMedicationDispenedPresciberPhoneReader = null;
                }
                if (oBodyPharmacistReader != null)
                {
                    oBodyPharmacistReader.Close();
                    oBodyPharmacistReader = null;
                }
                if (oBodyPharmacyNCPDPIDReader != null)
                {
                    oBodyPharmacyNCPDPIDReader.Close();
                    oBodyPharmacyNCPDPIDReader = null;
                }
                if (oBodyMedicationDispenedDrugUseEvaluationReader != null)
                {
                    oBodyMedicationDispenedDrugUseEvaluationReader.Close();
                    oBodyMedicationDispenedDrugUseEvaluationReader = null;
                }
                if (oBodyMedicationDispenedDrugUseEvaluationCoAgentReader != null)
                {
                    oBodyMedicationDispenedDrugUseEvaluationCoAgentReader.Close();
                    oBodyMedicationDispenedDrugUseEvaluationCoAgentReader = null;
                }
                if (oBodyPresciberReaderPReader != null)
                {
                    oBodyPresciberReaderPReader.Close();
                    oBodyPresciberReaderPReader = null;
                }
                if (oBodyPresciberSpecialtyReader != null)
                {
                    oBodyPresciberSpecialtyReader.Close();
                    oBodyPresciberSpecialtyReader = null;
                }
                if (oBodyPresciberAMASpecialtyReader != null)
                {
                    oBodyPresciberAMASpecialtyReader.Close();
                    oBodyPresciberAMASpecialtyReader = null;
                }
                if (oBodyPresciberOtherSpecialityReader != null)
                {
                    oBodyPresciberOtherSpecialityReader.Close();
                    oBodyPresciberOtherSpecialityReader = null;
                }
                if (oBodyPresciberReaderPNoReader != null)
                {
                    oBodyPresciberReaderPNoReader.Close();
                    oBodyPresciberReaderPNoReader = null;
                }
                if (oBodyMedicationDispenedPharmacyReader != null)
                {
                    oBodyMedicationDispenedPharmacyReader.Close();
                    oBodyMedicationDispenedPharmacyReader = null;
                }
                if (oBodyMedicationDispenedPresciberReader != null)
                {
                    oBodyMedicationDispenedPresciberReader.Close();
                    oBodyMedicationDispenedPresciberReader = null;
                }
                if (oBodyMedicationDispenedPresciberAddrReader != null)
                {
                    oBodyMedicationDispenedPresciberAddrReader.Close();
                    oBodyMedicationDispenedPresciberAddrReader = null;
                }
                if (oBodyMedicationDispenedPresciberNameReader != null)
                {
                    oBodyMedicationDispenedPresciberNameReader.Close();
                    oBodyMedicationDispenedPresciberNameReader = null;
                }
                if (oBodyMedicationDispenedPharmacyAddrReader != null)
                {
                    oBodyMedicationDispenedPharmacyAddrReader.Close();
                    oBodyMedicationDispenedPharmacyAddrReader = null;
                }
                if (oBodyMedicationDispenedPharmacyPNoReader != null)
                {
                    oBodyMedicationDispenedPharmacyPNoReader.Close();
                    oBodyMedicationDispenedPharmacyPNoReader = null;
                }
                if (oBodyMedicationDispenedPresciberIdReader != null)
                {
                    oBodyMedicationDispenedPresciberIdReader.Close();
                    oBodyMedicationDispenedPresciberIdReader = null;
                }
                if (oBodyPresciberIdReader != null)
                {
                    oBodyPresciberIdReader.Close();
                    oBodyPresciberIdReader = null;
                }
            }

            catch (Exception ex)
            {
                gloAuditTrail.gloAuditTrail.ExceptionLog(gloAuditTrail.ActivityModule.Prescription, gloAuditTrail.ActivityCategory.CreatePrescription, gloAuditTrail.ActivityType.General, ex.ToString(), gloAuditTrail.ActivityOutCome.Failure);
                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }
            //SLR: Disconncet, frree oclsRxHubDblayer, if it is not used again

            return _fResult;

        }
        #endregion "Rx History request and Response region "

        #region "For testing change function"
        public void GenerateRxHistoryRequestTest(string mPBM_PayerParticipantID, string mPBM_PayerName, string mCardHolderID, string mCardHolderName, string mGroupID, string mPBM_PayerMemberID, string RxHubParticipantId, string RxHubPassword)
        {
            string strRxHistoryResponseFileName = "";
            Int64 nPatientID;
            StringBuilder strInfo = new StringBuilder();
            try
            {
                string strOutbox = gloSettings.FolderSettings.AppTempFolderPath + "Outbox";

                //check and create directories if necessary 
                if (!Directory.Exists(strOutbox))
                {
                    //create it if it doesn't 
                    Directory.CreateDirectory(strOutbox);
                }

                //  string strRxHISResponseFileName = "RxHIS_" + System.DateTime.Now.Month.ToString("MM") + System.DateTime.Now.Day.ToString("dd") + System.DateTime.Now.Year.ToString("yyyy") + System.DateTime.Now.Hour.ToString("hh") + System.DateTime.Now.Minute.ToString("mm") + System.DateTime.Now.Second.ToString("ss") + ".xml";
                string strRxHISResponseFileName = "RxHIS_" + gloGlobal.clsFileExtensions.GetUniqueDateString("yyyyMMddHHmmssffff") + ".xml";


                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.NewLineOnAttributes = true;
                
                //create and update xml 
                strRxHistoryResponseFileName = strOutbox + "\\" + strRxHISResponseFileName;
                using (XmlWriter writer = XmlWriter.Create(strRxHistoryResponseFileName, settings))
                {
                    //DataTable dt = GetFormularlyCheckInfo(oPatient.PatientID);
                    //if (dt != null)
                    //{
                    oPatient.RxH271Master.PayerParticipantId = mPBM_PayerParticipantID;
                    oPatient.RxH271Master.PayerName = mPBM_PayerName;
                    oPatient.RxH271Master.CardHolderId = mCardHolderID;
                    oPatient.RxH271Master.CardHolderName = mCardHolderName;
                    oPatient.RxH271Master.GroupId = mGroupID;
                    oPatient.RxH271Master.MemberID = mPBM_PayerMemberID;
                    //}

                    nPatientID = oPatient.PatientID;
                    //main block Patient 
                    writer.WriteStartElement("Message", "http://www.ncpdp.org/schema/SCRIPT");
                    writer.WriteAttributeString("release", "001");
                    writer.WriteAttributeString("version", "008");


                    writer.WriteStartElement("Header");

                    writer.WriteStartElement("To");
                    writer.WriteAttributeString("Qualifier", "ZZZ");
                    writer.WriteValue(oPatient.RxH271Master.PayerParticipantId);
                    //writer.WriteValue("T00000000001000");



                    writer.WriteEndElement();

                    writer.WriteStartElement("From");
                    writer.WriteAttributeString("Qualifier", "ZZZ");
                    writer.WriteValue(RxHubParticipantId);//"T00000000020315"
                    writer.WriteEndElement();

                    writer.WriteElementString("MessageID", DateTime.Now.ToString("yyyyMMddhhmmss"));
                    //  writer.WriteElementString("SentTime", System.DateTime.Now.Year.ToString() + "-" + System.DateTime.Now.Month.ToString() + "-" + System.DateTime.Now.Day.ToString() + "T" + System.DateTime.Now.Hour.ToString() + ":" + System.DateTime.Now.Minute.ToString() + ":" + System.DateTime.Now.Second.ToString());
                    writer.WriteElementString("SentTime", System.DateTime.Now.ToString("yyyy-MM-dd") + "T" + System.DateTime.Now.ToString("hh:mm:ss"));


                    writer.WriteStartElement("Security");
                    writer.WriteStartElement("UsernameToken");
                    writer.WriteStartElement("Username");
                    writer.WriteEndElement();
                    writer.WriteEndElement();

                    writer.WriteStartElement("Sender");
                    writer.WriteElementString("SecondaryIdentification", RxHubPassword);//"FXTXGJVZ0W"
                    writer.WriteEndElement();

                    writer.WriteStartElement("Receiver");
                    writer.WriteElementString("SecondaryIdentification", "RxHUB");
                    writer.WriteEndElement();
                    writer.WriteEndElement();

                    if (ClsgloRxHubGeneral.gblnIsRxhubStagingServer == true)//testing
                    {
                        writer.WriteElementString("TestMessage", "1");  // "1" is for testing data else anything value is for live data
                    }
                    else//production
                    {
                        writer.WriteElementString("TestMessage", "2");  // "1" is for testing data else anything value is for live data
                    }


                    writer.WriteEndElement();

                    writer.WriteStartElement("Body");
                    writer.WriteStartElement("RxHistoryRequest");

                    writer.WriteStartElement("Prescriber");
                    writer.WriteStartElement("Identification");
                    if (oPatient.Provider.ProviderDEA != "" && oPatient.Provider.ProviderNPI != "")
                    {
                        writer.WriteElementString("DEANumber", oPatient.Provider.ProviderDEA);//"1720049901"oPatient.Provider.ProviderDEA
                        writer.WriteElementString("NPI", oPatient.Provider.ProviderNPI);//"1720049901"oPatient.Provider.ProviderDEA
                    }
                    else if (oPatient.Provider.ProviderDEA != "")
                    {
                        writer.WriteElementString("DEANumber", oPatient.Provider.ProviderDEA);//"1720049901"oPatient.Provider.ProviderDEA
                    }
                    else if (oPatient.Provider.ProviderNPI != "")
                    {
                        writer.WriteElementString("NPI", oPatient.Provider.ProviderNPI);//"1720049901"oPatient.Provider.ProviderDEA
                    }
                    else
                    {
                        MessageBox.Show("Invalid medication history request will be posted since the provider does not have either DEA or NPI information", "gloEMR", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    writer.WriteEndElement();
                    //writer.WriteElementString("ClinicName", oPatient.Provider.ClinicName);
                    // ' writer.WriteEndElement() 

                    writer.WriteStartElement("Name");
                    if (oPatient.Provider.ProviderLastName.ToString().Length > 35)
                    {
                        strInfo.Append("Provider Last Name(35),");
                        writer.WriteElementString("LastName", oPatient.Provider.ProviderLastName.ToString().Substring(0, 35));

                    }
                    else
                    {
                        writer.WriteElementString("LastName", oPatient.Provider.ProviderLastName);
                    }
                    //writer.WriteElementString("LastName", oPatient.Provider.ProviderLastName);

                    if (oPatient.Provider.ProviderFirstName.ToString().Length > 35)
                    {
                        strInfo.Append("Provider First Name(35),");
                        writer.WriteElementString("FirstName", oPatient.Provider.ProviderFirstName.ToString().Substring(0, 35));
                    }
                    else
                    {
                        writer.WriteElementString("FirstName", oPatient.Provider.ProviderFirstName);
                    }
                    //writer.WriteElementString("FirstName", oPatient.Provider.ProviderFirstName);

                    writer.WriteElementString("MiddleName", oPatient.Provider.ProviderMiddleName);

                    //writer.WriteElementString("Prefix", oPatient.Provider.ProviderPrefix);
                    if (oPatient.Provider.ProviderPrefix.ToString().Length > 10)
                    {
                        strInfo.Append("Prefix(10),");
                        writer.WriteElementString("Prefix", oPatient.Provider.ProviderPrefix.ToString().Substring(0, 10));
                    }
                    else
                    {
                        writer.WriteElementString("Prefix", oPatient.Provider.ProviderPrefix);
                    }

                    writer.WriteEndElement();

                    writer.WriteStartElement("Address");
                    if (oPatient.Provider.ProviderAddress.AddressLine1.ToString().Length > 35)
                    {
                        strInfo.Append("Provider Address Line 1(35),");
                        writer.WriteElementString("AddressLine1", oPatient.Provider.ProviderAddress.AddressLine1.ToString().Substring(0, 35));
                    }
                    else
                    {
                        writer.WriteElementString("AddressLine1", oPatient.Provider.ProviderAddress.AddressLine1);
                    }

                    //writer.WriteElementString("AddressLine1", oPatient.Provider.ProviderAddress.AddressLine1);
                    if (oPatient.Provider.ProviderAddress.AddressLine2.ToString().Length > 35)
                    {
                        strInfo.Append("Provider Address Line 2(35),");
                        writer.WriteElementString("AddressLine2", oPatient.Provider.ProviderAddress.AddressLine2.ToString().Substring(0, 35));
                    }
                    else
                    {
                        writer.WriteElementString("AddressLine2", oPatient.Provider.ProviderAddress.AddressLine2);
                    }
                    //writer.WriteElementString("AddressLine2", oPatient.Provider.ProviderAddress.AddressLine2);

                    //writer.WriteElementString("City", oPatient.Provider.ProviderAddress.City);
                    if (oPatient.Provider.ProviderAddress.City.ToString().Length > 35)
                    {
                        strInfo.Append("Provider City(35),");
                        writer.WriteElementString("City", oPatient.Provider.ProviderAddress.City.ToString().Substring(0, 35));
                    }
                    else
                    {
                        writer.WriteElementString("City", oPatient.Provider.ProviderAddress.City);
                    }

                    writer.WriteElementString("State", oPatient.Provider.ProviderAddress.State);
                    if (oPatient.Provider.ProviderAddress.Zip != "" && oPatient.Provider.ProviderAddress.Zip.Length > 5)
                    {
                        strInfo.Append("Provider Zip,");
                        oPatient.Provider.ProviderAddress.Zip = oPatient.Provider.ProviderAddress.Zip.Substring(0, 5);
                    }
                    writer.WriteElementString("ZipCode", oPatient.Provider.ProviderAddress.Zip);
                    //writer.WriteElementString("PlaceLocationQualifier", "");
                    writer.WriteEndElement();

                    writer.WriteEndElement(); //Prescriber ends here

                    writer.WriteStartElement("Patient");
                    writer.WriteElementString("PatientRelationship", "1");

                    writer.WriteStartElement("Name");

                    if (oPatient.LastName.ToString().Length > 35)
                    {
                        strInfo.Append("Patient Last Name(35),");
                        writer.WriteElementString("LastName", oPatient.LastName.ToString().Substring(0, 35));

                    }
                    else
                    {
                        writer.WriteElementString("LastName", oPatient.LastName);
                    }

                    if (oPatient.FirstName.ToString().Length > 35)
                    {
                        strInfo.Append("Patient First Name(35),");
                        writer.WriteElementString("FirstName", oPatient.FirstName.ToString().Substring(0, 35));
                    }
                    else
                    {
                        writer.WriteElementString("FirstName", oPatient.FirstName);
                    }

                    writer.WriteElementString("MiddleName", oPatient.MiddleName);
                    writer.WriteEndElement();

                    string sgender = oPatient.Gender;
                    if (sgender == "Male")
                    {
                        sgender = "M";
                    }
                    else
                    {
                        if (sgender == "Female")
                        {
                            sgender = "F";
                        }
                        else
                        {
                            sgender = "U";
                        }
                    }
                    writer.WriteElementString("Gender", sgender);
                    System.String dateOfBirth = "";
                    //DateTime formatedDOB = default(DateTime);
                    //formatedDOB = Convert.ToDateTime(dateOfBirth.Year + "-" + dateOfBirth.Month + "-" + dateOfBirth.Day);

                    //dateOfBirth = oPatient.DOB.Year.ToString() + "-" + oPatient.DOB.Month.ToString() + "-" + oPatient.DOB.Day.ToString();
                    dateOfBirth = oPatient.DOB.ToString("yyyy-MM-dd");

                    writer.WriteElementString("DateOfBirth", dateOfBirth);
                    //oPatient.DOB) 

                    writer.WriteStartElement("Address");

                    if (oPatient.PatientAddress.AddressLine1.ToString().Length > 35)
                    {
                        strInfo.Append("Patient Address Line 1(35),");
                        writer.WriteElementString("AddressLine1", oPatient.PatientAddress.AddressLine1.ToString().Substring(0, 35));
                    }
                    else
                    {
                        writer.WriteElementString("AddressLine1", oPatient.PatientAddress.AddressLine1);
                    }


                    //writer.WriteElementString("AddressLine2", oPatient.PatientAddress.AddressLine2);
                    if (oPatient.PatientAddress.City.ToString().Length > 35)
                    {
                        strInfo.Append("Patient City(35),");
                        writer.WriteElementString("City", oPatient.PatientAddress.City.ToString().Substring(0, 35));
                    }
                    else
                    {
                        writer.WriteElementString("City", oPatient.PatientAddress.City);
                    }
                    //writer.WriteElementString("City", oPatient.PatientAddress.City);
                    writer.WriteElementString("State", oPatient.PatientAddress.State);
                    if (oPatient.PatientAddress.Zip != "" && oPatient.PatientAddress.Zip.Length > 5)
                    {
                        strInfo.Append("Patient Zip(5),");
                        oPatient.PatientAddress.Zip = oPatient.PatientAddress.Zip.Substring(0, 5);
                    }
                    writer.WriteElementString("ZipCode", oPatient.PatientAddress.Zip);

                    //writer.WriteElementString("PlaceLocationQualifier", "");
                    writer.WriteEndElement();

                    writer.WriteEndElement(); //Patient tag ends here


                    writer.WriteStartElement("BenefitsCoordination");
                    writer.WriteStartElement("PayerIdentification");
                    writer.WriteElementString("PayerID", oPatient.RxH271Master.PayerParticipantId);
                    // oRxH271Master.PayerParticipantId)'oEligibilityCheck.Patient.Subscriber.Item(i).SubscriberID) 
                    writer.WriteEndElement(); // end PayerIdentification
                    writer.WriteElementString("PayerName", oPatient.RxH271Master.PayerName);
                    //"InsuranceCompanyName") 

                    if (oPatient.RxH271Master.CardHolderId.ToString().Length > 35)
                    {
                        writer.WriteElementString("CardholderID", oPatient.RxH271Master.CardHolderId.ToString().Substring(0, 35));
                    }
                    else
                    {
                        writer.WriteElementString("CardholderID", oPatient.RxH271Master.CardHolderId);
                    }

                    // 
                    writer.WriteElementString("CardholderName", oPatient.RxH271Master.CardHolderName);


                    if (oPatient.RxH271Master.GroupId.ToString().Length > 35)
                    {
                        writer.WriteElementString("GroupID", oPatient.RxH271Master.GroupId.ToString().Substring(0, 35));
                    }
                    else
                    {
                        writer.WriteElementString("GroupID", oPatient.RxH271Master.GroupId);
                    }

                    writer.WriteElementString("Consent", Consent);//oPatient.RxH271Master.Consent);
                    writer.WriteElementString("PBMMemberID", oPatient.RxH271Master.MemberID);
                    // 
                    //BenefitsCoordination 
                    writer.WriteEndElement();


                    writer.WriteEndElement();
                    //RxHistoryRequest 
                    writer.WriteEndElement();
                    //Body 
                    writer.WriteEndElement();
                    //Message 
                    writer.Close();
                    string strMessage = "";
                    if (strInfo.Length > 0)
                    {
                        if (!string.IsNullOrEmpty(strInfo.ToString().Trim()))
                        {
                            if (strInfo.ToString().EndsWith(","))
                            {

                                strMessage = strInfo.ToString().Trim().Substring(0, ((strInfo.ToString().Trim().Length) - 1));
                                strInfo = new System.Text.StringBuilder();
                                strInfo.Append(strMessage);
                            }
                        }
                    }
                    if (strInfo.ToString().Length > 0)
                    {
                        if (MessageBox.Show("Following data fields exceed number of characters allowed in x12 standards and will therefore be truncated before sending to Surescripts and PBM�s (Allowed characters are shown in parenthesis) " + System.Environment.NewLine + System.Environment.NewLine + System.Environment.NewLine + strMessage + System.Environment.NewLine + System.Environment.NewLine + System.Environment.NewLine + "Do you want to continue?", "gloEMR", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == System.Windows.Forms.DialogResult.No)
                        {
                            return;
                        }
                    }

                    strRxHisReqPath = strRxHistoryResponseFileName;
                    
                }

                //assigning values to the opatient object for saving it into the RxH_HistoryMsgHeader table against genrated request 
                oPatient.MessageHeader.MessageDescription = "RxHistoryRequest";
                oPatient.MessageHeader.To = oPatient.RxH271Master.PayerParticipantId;
                oPatient.MessageHeader.From = RxHubParticipantId;//"T00000000020315"
                oPatient.MessageHeader.SecuritySecondaryID = RxHubPassword;//"FXTXGJVZ0W"
                oPatient.MessageHeader.SecurityReceiverSecID = "RxHUB";
                oPatient.MessageHeader.TestMessage = "1";
                oPatient.MessageHeader.SentTime = System.DateTime.Now.ToString("yyyy-MM-dd") + "T" + System.DateTime.Now.ToString("hh:mm:ss");
                oPatient.MessageHeader.MessageID = DateTime.Now.ToString("yyyyMMddhhmmss");

                //SLR: Disconncet, frree previousul allocated memory before allocating once more
                if (oclsgloRxHubDBLayer != null)
                {
                    oclsgloRxHubDBLayer.Disconnect();
                    oclsgloRxHubDBLayer.Dispose();
                    oclsgloRxHubDBLayer = null;
                }
                //save the Request data into the respected table
                oclsgloRxHubDBLayer = new clsgloRxHubDBLayer();
                oclsgloRxHubDBLayer.Connect(ClsgloRxHubGeneral.ConnectionString);
                oclsgloRxHubDBLayer.InsertRxH_HistoryHeader("gsp_InUpRxH_HistoryHeader", oPatient, nPatientID);
                oclsgloRxHubDBLayer.Disconnect();

                //SLR: Disconncet, frree previousul allocated memory before allocating once more
                if (oclsgloRxHubDBLayer != null)
                {
                    oclsgloRxHubDBLayer.Disconnect();
                    oclsgloRxHubDBLayer.Dispose();
                    oclsgloRxHubDBLayer = null;
                }
                //Save data into Request detail table
                oclsgloRxHubDBLayer = new clsgloRxHubDBLayer();
                oclsgloRxHubDBLayer.Connect(ClsgloRxHubGeneral.ConnectionString);
                oclsgloRxHubDBLayer.InsertRxH_HistoryRequestDetails("gsp_InUpRxH_HistoryRequest_DTL", oPatient);
                oclsgloRxHubDBLayer.Disconnect();
                
            }
            catch (Exception ex)
            {
                gloAuditTrail.gloAuditTrail.ExceptionLog(gloAuditTrail.ActivityModule.Prescription, gloAuditTrail.ActivityCategory.CreatePrescription, gloAuditTrail.ActivityType.General, ex.ToString(), gloAuditTrail.ActivityOutCome.Failure);
                throw ex;
                //return null;
            }
            finally
            {

            }
        }
        #endregion
        
        #region "Automate Rx Eligibility Request"


        public Cls271Information Translate271Response_auto()
        {
            return null;
        //    Cls271Information oCls271Information = new Cls271Information();
        //    ClsRxH_271Master oClsRxH_271Master = new ClsRxH_271Master();
        //    ClsRxH_271Details oClsRxH_271Details = new ClsRxH_271Details();
        //    oclsgloRxHubDBLayer = new clsgloRxHubDBLayer();
        //    try
        //    {
        //        Boolean blnNM1Found = false;//if it is 2B then only we will save the EB data
        //        Boolean blnContractedProvider = false;
        //        Boolean blnPrimaryPayer = false;

        //        //int STLoopCnt = 0;

        //        //string sISA_ControlNumber = "";
        //        string sSegmentID = "";
        //        string sLoopSection = "";
        //        //string sLXID = "";
        //        //string sPath = "";
        //        string sEntity = "";
        //        string Qlfr = "";
        //        string Qlfr_InsuranceTypeCode = "";//if present then either  it will be 47/CP/MC/MP/OT
        //        string sRecieverID = "";
        //        string sSenderID = "";
        //        string sMemberID = "";
        //        string sEmployeeId = "";
        //        string sPlanNumber = "";
        //        string sGroupNumber = "";
        //        string sFormularlyID = "";
        //        string sAlternativeID = "";
        //        string sBIN = "";
        //        //string sPCN = "";
        //        string sdtEligiblityDate = "12:00:00 AM";
        //        //bool IsDemographicChange = false;
        //        bool IsPharmacy = false;
        //        bool IsMailOrdRx = false;

        //        //string strRejectionCode = "";
        //        //string strFollowupCode = "";

        //        int nArea;

        //        StringBuilder sValue = new StringBuilder();
        //        //Int32 _nArea2RowCount = 0;
        //        //int Area2rowIndex = 0;
        //        //int rowIndex = 0;
        //        //int i = 0;
        //        // Gets the first data segment in the EDI files
        //        ediDataSegment.Set(ref oSegment, (ediDataSegment)oEdiDoc.FirstDataSegment);  //oSegment = (ediDataSegment) oEdiDoc.FirstDataSegment

        //        oClsRxH_271Master.PatientId = _PatientId;//gloRxHub.ClsgloRxHubGeneral.gnPatientId;

        //        // This loop iterates though the EDI file a segment at a time
        //        while (oSegment != null)
        //        {
        //            // A segment is identified by its Area number, Loop section and segment id.
        //            sSegmentID = oSegment.ID;
        //            sLoopSection = oSegment.LoopSection;
        //            nArea = oSegment.Area;

        //            if (nArea == 0)
        //            {
        //                if (sLoopSection == "")
        //                {
        //                    if (sSegmentID == "ISA")
        //                    {
        //                        // map data elements of ISA segment in here

        //                        sValue.Append("Authorization Information Qualifier :" + oSegment.get_DataElementValue(1) + Environment.NewLine);    //Authorization Information Qualifier
        //                        sValue.Append("Authorization Information :" + oSegment.get_DataElementValue(2) + Environment.NewLine);    //Authorization Information
        //                        sValue.Append("Security Information Qualifier :" + oSegment.get_DataElementValue(3) + Environment.NewLine);    //Security Information Qualifier
        //                        sValue.Append("Security Information :" + oSegment.get_DataElementValue(4) + Environment.NewLine);    //Security Information
        //                        sValue.Append("Interchange ID Qualifier :" + oSegment.get_DataElementValue(5) + Environment.NewLine);    //Interchange ID Qualifier
        //                        sValue.Append("Interchange Sender ID :" + oSegment.get_DataElementValue(6) + Environment.NewLine);    //Interchange Sender ID
        //                        sSenderID = oSegment.get_DataElementValue(6).Trim();
        //                        sValue.Append("Sender ID :" + sSenderID + Environment.NewLine);    //Interchange Sender ID
        //                        sValue.Append("Interchange ID Qualifier :" + oSegment.get_DataElementValue(7) + Environment.NewLine);    //Interchange ID Qualifier
        //                        sValue.Append("Interchange Receiver ID" + oSegment.get_DataElementValue(8) + Environment.NewLine);    //Interchange Receiver ID
        //                        sRecieverID = oSegment.get_DataElementValue(8).Trim();
        //                        sValue.Append("Receiver ID" + sRecieverID + Environment.NewLine);
        //                        sValue.Append("Interchange Date :" + oSegment.get_DataElementValue(9) + Environment.NewLine);    //Interchange Date
        //                        sValue.Append("Interchange Time :" + oSegment.get_DataElementValue(10) + Environment.NewLine);   //Interchange Time
        //                        sValue.Append("Interchange Control Standards Identifier :" + oSegment.get_DataElementValue(11) + Environment.NewLine);   //Interchange Control Standards Identifier
        //                        sValue.Append("Interchange Control Version Number :" + oSegment.get_DataElementValue(12) + Environment.NewLine);   //Interchange Control Version Number
        //                        sValue.Append("Interchange Control Number :" + oSegment.get_DataElementValue(13) + Environment.NewLine);   //Interchange Control Number
        //                        //This will be saved as message id in database
        //                        oClsRxH_271Master.ISA_ControlNumber = oSegment.get_DataElementValue(13);
        //                        sValue.Append("Acknowledgment Requested :" + oSegment.get_DataElementValue(14) + Environment.NewLine);   //Acknowledgment Requested
        //                        sValue.Append("Usage Indicator :" + oSegment.get_DataElementValue(15) + Environment.NewLine);   //Usage Indicator
        //                        sValue.Append("Component Element Separator :" + oSegment.get_DataElementValue(16) + Environment.NewLine);   //Component Element Separator

        //                    }
        //                    //TA1 segment
        //                    else if (sSegmentID == "TA1")
        //                    {
        //                        //check for the code in the 5th element
        //                        oClsRxH_271Master.MessageType = "TA1" + "-" + oSegment.get_DataElementValue(04);
        //                        if (oSegment.get_DataElementValue(04) == "A")
        //                        {
        //                            //MessageBox.Show("The Transmitted Interchange Control Structure Header and Trailer Have Been Received and Have No Errors.", "gloRxHub", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //                            clsRxGeneral.UpdateLogForRx("The Transmitted Interchange Control Structure Header and Trailer Have Been Received and Have No Errors.");
        //                        }
        //                        else if (oSegment.get_DataElementValue(04) == "R")
        //                        {
        //                            StringBuilder strTA1Msg = new StringBuilder();
        //                            strTA1Msg.Append("The Transmitted Interchange Control Structure Header");
        //                            strTA1Msg.Append(Environment.NewLine);
        //                            strTA1Msg.Append("and Trailer Have Been Received and Are Accepted But");
        //                            strTA1Msg.Append(Environment.NewLine);
        //                            strTA1Msg.Append("Errors Are Noted. This Means the Sender Must Not Resend This Data.");
        //                          //  MessageBox.Show(strTA1Msg.ToString(), "gloRxHub", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //                            clsRxGeneral.UpdateLogForRx(strTA1Msg.ToString());
        //                        }
        //                        else if (oSegment.get_DataElementValue(04) == "E")
        //                        {
        //                           // MessageBox.Show("The Transmitted Interchange Control Structure Header and Trailer are Rejected Because of Errors.", "gloRxHub", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //                            clsRxGeneral.UpdateLogForRx("The Transmitted Interchange Control Structure Header and Trailer are Rejected Because of Errors.");
        //                        }
        //                        oclsgloRxHubDBLayer.Connect(ClsgloRxHubGeneral.ConnectionString);
        //                        oclsgloRxHubDBLayer.InsertEDIResponse271_Details("gsp_InUpRxH_RxH_271Response_Details", oClsRxH_271Master);
        //                        //return null;
        //                        break;
        //                    }

        //                    else if (sSegmentID == "GS")
        //                    {
        //                        // map data elements of GS segment in here
        //                        sValue.Append("Functional Identifier Code :" + oSegment.get_DataElementValue(1) + Environment.NewLine);  //Functional Identifier Code
        //                        sValue.Append("Application Sender's Code :" + oSegment.get_DataElementValue(2) + Environment.NewLine);  //Application Sender's Code
        //                        sValue.Append("Application Receiver's Code :" + oSegment.get_DataElementValue(3) + Environment.NewLine);  //Application Receiver's Code
        //                        sValue.Append("Date :" + oSegment.get_DataElementValue(4) + Environment.NewLine);  //Date
        //                        sValue.Append("Time :" + oSegment.get_DataElementValue(5) + Environment.NewLine);  //Time
        //                        sdtEligiblityDate = oSegment.get_DataElementValue(4).Trim() + " " + oSegment.get_DataElementValue(5).Trim();
        //                        sValue.Append("Eligiblity Date : " + sdtEligiblityDate + Environment.NewLine);
        //                        sValue.Append("Group Control Number :" + oSegment.get_DataElementValue(6) + Environment.NewLine);  //Group Control Number
        //                        sValue.Append("Responsible Agency Code :" + oSegment.get_DataElementValue(7) + Environment.NewLine);  //Responsible Agency Code
        //                        sValue.Append("Version :" + oSegment.get_DataElementValue(8));  //Version / Release
        //                    }
        //                }
        //            }
        //            else if (nArea == 1)
        //            {
        //                if (sLoopSection == "")
        //                {
        //                    if (sSegmentID == "ST")
        //                    {
        //                        // map data element of ST segment in here
        //                        if (oSegment.get_DataElementValue(1) != "")//means we have either 271 / 997
        //                        {
        //                            if (oSegment.get_DataElementValue(1) == "997")
        //                            {
        //                                oClsRxH_271Master.MessageType = oSegment.get_DataElementValue(1);//either 271 / 997
        //                               // MessageBox.Show("The 270 eligibility request file contains Error, 997 file responded!", "gloRxHub", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //                                clsRxGeneral.UpdateLogForRx("The 270 eligibility request file contains Error, 997 file responded!");
        //                                oclsgloRxHubDBLayer.Connect(ClsgloRxHubGeneral.ConnectionString);
        //                                oclsgloRxHubDBLayer.InsertEDIResponse271_Details("gsp_InUpRxH_RxH_271Response_Details", oClsRxH_271Master);
        //                                break;
        //                            }
        //                            else
        //                            {
        //                                oClsRxH_271Master.MessageType = oSegment.get_DataElementValue(1);//either 271 / 997
        //                                oClsRxH_271Master.STLoopCount = oSegment.get_DataElementValue(2);// STLoopCnt + 1;// oSegment.get_DataElementValue(2); //0001
        //                            }

        //                        }
        //                        else
        //                        {
        //                            oClsRxH_271Master.MessageType = "TA1";//wil be TA1. or NAK
        //                        }

        //                        sValue.Append(oSegment.get_DataElementValue(1) + Environment.NewLine);
        //                        sValue.Append(oSegment.get_DataElementValue(2) + Environment.NewLine); //00021
        //                    }
        //                    else if (sSegmentID == "BHT")
        //                    {
        //                        sValue.Append(oSegment.get_DataElementValue(3) + Environment.NewLine);
        //                        //this wil be saved as transaction ID in Database
        //                        oClsRxH_271Master.MessageID = oSegment.get_DataElementValue(3);
        //                    }
        //                    else if (sSegmentID == "REF")
        //                    {

        //                    }
        //                }

        //            }//Area ==1

        //            else if (nArea == 2)
        //            {
        //                if (sLoopSection == "HL" && sSegmentID == "HL")
        //                {
        //                    sEntity = oSegment.get_DataElementValue(3);
        //                }
        //                //****************************** Information Source 
        //                if (sEntity == "20")
        //                {
        //                    if (sLoopSection == "HL")
        //                    {
        //                        if (sSegmentID == "HL")
        //                        {

        //                        }
        //                        else if (sSegmentID == "AAA")
        //                        {

        //                        }

        //                    }//end loop section HL

        //                    else if (sLoopSection == "HL;NM1")
        //                    {

        //                        if (sSegmentID == "NM1")
        //                        {



        //                            //txtInformationSourceName.Text = oSegment.get_DataElementValue(3);//Information Source Name
        //                            oClsRxH_271Master.InformationSourceName = oSegment.get_DataElementValue(3) + " " + oSegment.get_DataElementValue(4) + " " + oSegment.get_DataElementValue(5);//PBM or payer name
        //                            oClsRxH_271Master.PayerName = oSegment.get_DataElementValue(3);//PBM or payer name
        //                            oClsRxH_271Master.PayerParticipantId = oSegment.get_DataElementValue(9);//PBM or payer participant ID returned in the NM109 will be used to populate drug history request transactions.
        //                        }
        //                        else if (sSegmentID == "REF")
        //                        {

        //                        }
        //                        else if (sSegmentID == "PER")
        //                        {


        //                        }
        //                        else if (sSegmentID == "AAA")
        //                        {
        //                            if (oSegment.get_DataElementValue(1) == "N")
        //                            {
        //                                sValue.Append(oSegment.get_DataElementValue(2) + Environment.NewLine);
        //                                if (oSegment.get_DataElementValue(3).Trim() != "")
        //                                {
        //                                    //listResponse.Items.Add("Payer Rejection Reason: " + GetSourceRejectionReason(oSegment.get_DataElementValue(3)));
        //                                    //listResponse.Items.Add("Payer Follow up: " + GetSourceFollowUp(oSegment.get_DataElementValue(4)));
        //                                }

        //                                EDIReturnResult = oSegment.get_DataElementValue(3).Trim() + "-" + oSegment.get_DataElementValue(4).Trim();
        //                                //AddNote(_PatientId, EDIReturnResult);//
        //                            }
        //                        }

        //                    }//end loop section HL;NM1

        //                }

        //                    //**************************** Information Reciever - physician
        //                else if (sEntity == "21")
        //                {
        //                    if (sLoopSection == "HL")
        //                    {

        //                        if (sSegmentID == "HL")
        //                        {
        //                            sValue.Append(oSegment.get_DataElementValue(1) + Environment.NewLine);
        //                            sValue.Append(oSegment.get_DataElementValue(2) + Environment.NewLine);
        //                            sValue.Append(oSegment.get_DataElementValue(3) + Environment.NewLine);
        //                        }
        //                    }

        //                    else if (sLoopSection == "HL;NM1")
        //                    {

        //                        if (sSegmentID == "NM1")
        //                        {
        //                            //txtInformationRecieverName.Text = oSegment.get_DataElementValue(3);//Information Reciever Name
        //                            string RecieverLastName = oSegment.get_DataElementValue(3);
        //                            string RecieverFirstName = oSegment.get_DataElementValue(4);
        //                            string RecieverMiddleName = oSegment.get_DataElementValue(5);
        //                            oClsRxH_271Master.InformationRecieverName = RecieverLastName + " " + RecieverFirstName + " " + RecieverMiddleName;//Information Reciever Name
        //                            oClsRxH_271Master.InformationRecieverSuffix = oSegment.get_DataElementValue(7);//MD
        //                            sValue.Append("Payer :" + oSegment.get_DataElementValue(1) + Environment.NewLine);//PR=Payer
        //                            sValue.Append("Non-Person Entity :" + oSegment.get_DataElementValue(2) + Environment.NewLine);//2=Non-Person Entity
        //                            sValue.Append("Information Source Name :" + oSegment.get_DataElementValue(3) + Environment.NewLine);//"INFORMATION SOURCE NAME" );//"PBM"
        //                            //txtPBMPayerName.Text = oSegment.get_DataElementValue(3);//++++++++++++++++++++
        //                            //oClsRxH_271Master.PayerName = oSegment.get_DataElementValue(3) + " " + oSegment.get_DataElementValue(4) + " " + oSegment.get_DataElementValue(5);//++++++++++++++++++++
        //                            sValue.Append("Payer Identification :" + oSegment.get_DataElementValue(8) + Environment.NewLine);//PI=Payer Identification
        //                            string str = oSegment.get_DataElementValue(4);
        //                            sValue.Append("PayerID :" + oSegment.get_DataElementValue(9) + Environment.NewLine);//PayerID
        //                            //txtPayerParticipantId.Text = oSegment.get_DataElementValue(9);//++++++++++++++++++++
        //                            //oClsRxH_271Master.PayerParticipantId = oSegment.get_DataElementValue(9);//++++++++++++++++++++

        //                        }
        //                        else if (sSegmentID == "REF")
        //                        {

        //                            sValue.Append(oSegment.get_DataElementValue(1) + Environment.NewLine);
        //                            sValue.Append(oSegment.get_DataElementValue(2) + Environment.NewLine);
        //                            sValue.Append(oSegment.get_DataElementValue(3) + Environment.NewLine);

        //                        }
        //                        else if (sSegmentID == "AAA")
        //                        {
        //                            if (oSegment.get_DataElementValue(1) == "N")
        //                            {
        //                                sValue.Append(oSegment.get_DataElementValue(2) + Environment.NewLine);
        //                                if (oSegment.get_DataElementValue(3).Trim() != "")
        //                                {
        //                                    //listResponse.Items.Add("Receiver Rejection Reason: " + GetReceiverRejectionReason(oSegment.get_DataElementValue(3)));
        //                                    //listResponse.Items.Add("Receiver Follow up: " + GetReceiverFollowUp(oSegment.get_DataElementValue(4)));
        //                                }

        //                                EDIReturnResult = oSegment.get_DataElementValue(3).Trim() + "-" + oSegment.get_DataElementValue(4).Trim();
        //                                //AddNote(_PatientId, EDIReturnResult);
        //                                //txtProviderInfo.Text = GetReceiverRejectionReason(oSegment.get_DataElementValue(3));
        //                                //txtProviderResponse.Text = GetReceiverFollowUp(oSegment.get_DataElementValue(4));
        //                                //EDIReturnResult = EDIReturnResult + txtProviderInfo.Text.Trim() + "-" + txtProviderResponse.Text.Trim();
        //                                //AddNote(_PatientId, txtProviderInfo.Text.Trim() + "-" + txtProviderResponse.Text.Trim());
        //                            }
        //                        }
        //                    }

        //                }
        //                //*****************************Subscriber loop
        //                else if (sEntity == "22")
        //                {

        //                    if (sLoopSection == "HL")
        //                    {
        //                        if (sSegmentID == "HL")
        //                        {
        //                            sValue.Append(oSegment.get_DataElementValue(1) + Environment.NewLine);
        //                            sValue.Append(oSegment.get_DataElementValue(2) + Environment.NewLine);
        //                            sValue.Append(oSegment.get_DataElementValue(3) + Environment.NewLine);
        //                        }
        //                        else if (sSegmentID == "TRN")
        //                        {
        //                            sValue.Append(oSegment.get_DataElementValue(1) + Environment.NewLine);
        //                            sValue.Append(oSegment.get_DataElementValue(2) + Environment.NewLine);
        //                            sValue.Append(oSegment.get_DataElementValue(3) + Environment.NewLine);
        //                        }
        //                    }

        //                    else if (sLoopSection == "HL;NM1")
        //                    {
        //                        if (sSegmentID == "NM1")
        //                        {

        //                            sValue.Append("Subscriber Last Name : " + oSegment.get_DataElementValue(3) + Environment.NewLine);
        //                            //txtSubscriberLastName.Text = oSegment.get_DataElementValue(3);//Subscriber Last Name
        //                            oClsRxH_271Details.SubscriberLastName = oSegment.get_DataElementValue(3);//Subscriber Last Name

        //                            sValue.Append("Subscriber First Name : " + oSegment.get_DataElementValue(4) + Environment.NewLine);
        //                            //txtSubscriberFirstName.Text = oSegment.get_DataElementValue(4);//Subscriber First Name
        //                            oClsRxH_271Details.SubscriberFirstName = oSegment.get_DataElementValue(4);//Subscriber First Name

        //                            //txtSubscriberMiddleName.Text = oSegment.get_DataElementValue(5);//Subscriber Middle Name
        //                            oClsRxH_271Details.SubscriberMiddleName = oSegment.get_DataElementValue(5);//Subscriber Middle Name

        //                            //Subscriber Suffix e.g., Sr., Jr., or III.
        //                            oClsRxH_271Details.SubscriberSuffix = oSegment.get_DataElementValue(7);//Subscriber Suffix

        //                            sValue.Append("Subscriber ID: " + oSegment.get_DataElementValue(9) + Environment.NewLine);
        //                            //txtPBMPayerMemberId.Text = oSegment.get_DataElementValue(9);
        //                            oClsRxH_271Master.MemberID = oSegment.get_DataElementValue(9);//Patient PBM/payer unique member ID will be used to populate drug history request and new prescription transactions.
        //                        }
        //                        else if (sSegmentID == "N3")
        //                        {
        //                            sValue.Append("Subscriber Address : " + oSegment.get_DataElementValue(1) + Environment.NewLine);
        //                            //txtSubscriberAddressLine1.Text = oSegment.get_DataElementValue(1);//Subscriber Address Line1
        //                            oClsRxH_271Details.SubscriberAddress1 = oSegment.get_DataElementValue(1);//Subscriber Address Line1

        //                            //txtSubscriberAddressLine2.Text = oSegment.get_DataElementValue(2);//Subscriber Address Line2
        //                            oClsRxH_271Details.SubscriberAddress2 = oSegment.get_DataElementValue(2);//Subscriber Address Line2

        //                        }
        //                        else if (sSegmentID == "N4")
        //                        {
        //                            sValue.Append("Subscriber City : " + oSegment.get_DataElementValue(1) + Environment.NewLine);
        //                            //txtSubscriberCity.Text = oSegment.get_DataElementValue(1);//Subscriber City
        //                            oClsRxH_271Details.SubscriberCity = oSegment.get_DataElementValue(1);//Subscriber City

        //                            sValue.Append("Subscriber State : " + oSegment.get_DataElementValue(2) + Environment.NewLine);
        //                            //txtSubscriberState.Text = oSegment.get_DataElementValue(2);//Subscriber State
        //                            oClsRxH_271Details.SubscriberState = oSegment.get_DataElementValue(2);//Subscriber State

        //                            sValue.Append("Subscriber Zip : " + oSegment.get_DataElementValue(3) + Environment.NewLine);
        //                            //txtSubscriberZip.Text = oSegment.get_DataElementValue(3);//Subscriber Zip
        //                            oClsRxH_271Details.SubscriberZip = oSegment.get_DataElementValue(3);//Subscriber Zip

        //                        }
        //                        else if (sSegmentID == "PER")
        //                        {

        //                        }
        //                        else if (sSegmentID == "REF")
        //                        {
        //                            Qlfr = oSegment.get_DataElementValue(1);
        //                            //returned Employee ID
        //                            if (Qlfr == "A6")
        //                            {
        //                                sEmployeeId = oSegment.get_DataElementValue(2);
        //                                sValue.Append("Employee ID : " + sEmployeeId + Environment.NewLine);
        //                                oClsRxH_271Master.EmployeeId = oSegment.get_DataElementValue(2);
        //                            }
        //                            //If returned, health plan name in the REF03 must be displayed in the end user application per RxHub application guideline 3.6.
        //                            if (Qlfr == "18")
        //                            {
        //                                sPlanNumber = oSegment.get_DataElementValue(2);
        //                                sValue.Append("Plan Number : " + sPlanNumber + Environment.NewLine);
        //                                //txtHealthPlanName.Text = oSegment.get_DataElementValue(3);//++++++++++++++++++++
        //                                oClsRxH_271Master.HealthPlanNumber = oSegment.get_DataElementValue(2);
        //                                oClsRxH_271Master.HealthPlanName = oSegment.get_DataElementValue(3);//health plan name in the REF03 
        //                            }
        //                            //If returned, cardholder ID in the REF02 and cardholder name in the REF03 will be used to populate drug history request and new prescription transactions.
        //                            else if (Qlfr == "1W")
        //                            {
        //                                sMemberID = oSegment.get_DataElementValue(2);
        //                                sValue.Append("Member ID : " + sMemberID + Environment.NewLine);
        //                                //txtCardHolderId.Text = oSegment.get_DataElementValue(2);//++++++++++++++++++++
        //                                oClsRxH_271Master.CardHolderId = oSegment.get_DataElementValue(2);//cardholder ID in the REF02 
        //                                //txtCardHolderName.Text = oSegment.get_DataElementValue(2);//++++++++++++++++++++
        //                                oClsRxH_271Master.CardHolderName = oSegment.get_DataElementValue(3);//and cardholder name in the REF03 will be used to populate drug history request and new prescription transactions.
        //                            }
        //                            //If returned, group ID in the REF02 will be used to populate drug history request and new prescription transactions.
        //                            else if (Qlfr == "6P")
        //                            {
        //                                sGroupNumber = oSegment.get_DataElementValue(2);
        //                                sValue.Append("Group Number : " + sGroupNumber + Environment.NewLine);
        //                                //txtGroupId.Text = oSegment.get_DataElementValue(2);//++++++++++++++++++++
        //                                oClsRxH_271Master.GroupId = oSegment.get_DataElementValue(2);//group ID in the REF02 will be used to populate drug history request and new prescription transactions.
        //                                oClsRxH_271Master.GroupName = oSegment.get_DataElementValue(3);//group ID in the REF02 will be used to populate drug history request and new prescription transactions.
        //                            }
        //                            //If returned, formulary list ID in the REF02 and alternative list ID in the REF03 will be used to link to patient formulary and benefit data.
        //                            else if (Qlfr == "IF")
        //                            {
        //                                sFormularlyID = oSegment.get_DataElementValue(2);//formulary list ID in the REF02 
        //                                sValue.Append("Formulary ID : " + sFormularlyID + Environment.NewLine);
        //                                //txtFormularyListId.Text = oSegment.get_DataElementValue(2);//++++++++++++++++++++
        //                                oClsRxH_271Master.FormularyListId = oSegment.get_DataElementValue(2);//++++++++++++++++++++
        //                                sAlternativeID = oSegment.get_DataElementValue(3);
        //                                sValue.Append("Alternative ID : " + sAlternativeID + Environment.NewLine);
        //                                //txtAlternativeListId.Text = oSegment.get_DataElementValue(3);//++++++++++++++++++++
        //                                oClsRxH_271Master.AlternativeListId = oSegment.get_DataElementValue(3);//++++++++++++++++++++
        //                            }
        //                            //If returned, coverage ID in the REF03 will be used to link to patient formulary and benefit data.
        //                            else if (Qlfr == "1L")
        //                            {
        //                                //sFormularlyID = oSegment.get_DataElementValue(2);
        //                                //sValue.Append("Formulary ID : " + sFormularlyID + Environment.NewLine);
        //                                //txtCoverageId.Text = oSegment.get_DataElementValue(3);//++++++++++++++++++++
        //                                oClsRxH_271Master.CoverageId = oSegment.get_DataElementValue(3);//++++++++++++++++++++
        //                            }
        //                            //If returned, BIN number in the REF02 will be used to populate new prescription transactions.
        //                            else if (Qlfr == "N6")
        //                            {
        //                                sBIN = oSegment.get_DataElementValue(2);
        //                                sValue.Append("BIN : " + sBIN + Environment.NewLine);
        //                                //txtBinNumber.Text = oSegment.get_DataElementValue(2);//++++++++++++++++++++
        //                                oClsRxH_271Master.BINNumber = oSegment.get_DataElementValue(2);//++++++++++++++++++++
        //                                //sPCN = oSegment.get_DataElementValue(3);
        //                                //sValue.Append("PCN : " + sPCN + Environment.NewLine);
        //                            }
        //                            //If returned, copay ID in the REF03 will be used to link to patient formulary and benefit data.
        //                            else if (Qlfr == "IG")
        //                            {
        //                                //sBIN = oSegment.get_DataElementValue(2);
        //                                //sValue.Append("BIN : " + sBIN + Environment.NewLine);
        //                                //txtCopayId.Text = oSegment.get_DataElementValue(3);//++++++++++++++++++++
        //                                oClsRxH_271Master.CopayId = oSegment.get_DataElementValue(3);//++++++++++++++++++++
        //                                //sPCN = oSegment.get_DataElementValue(3);
        //                                //sValue.Append("PCN : " + sPCN + Environment.NewLine);
        //                            }

        //                        }
        //                        else if (sSegmentID == "AAA")
        //                        {
        //                            if (oSegment.get_DataElementValue(1) == "Y")
        //                            {
        //                                //check the 3rd value. if it is 67 then the patient is not present on RxHub database
        //                                if (oSegment.get_DataElementValue(3) == "67")
        //                                {
        //                                    if (oClsRxH_271Master.MessageType != "")
        //                                    {
        //                                        //we will append a flag against the message type only if the condition is of type "Patient Not Found" 
        //                                        //and so in this way this will help the user to resend the 270 eligiblity request again for this patient
        //                                        oClsRxH_271Master.MessageType = oClsRxH_271Master.MessageType + "|PNF";
        //                                    }

        //                              //      MessageBox.Show("The request is valid, however the transaction has been rejected. Patient Not Found", "gloRxhub", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        //                                    clsRxGeneral.UpdateLogForRx("The request is valid, however the transaction has been rejected. Patient Not Found");
        //                                    //break;
        //                                }
        //                                else if (oSegment.get_DataElementValue(3) == "41")//NCP = No contract with payer
        //                                {
        //                                    if (oClsRxH_271Master.MessageType != "")
        //                                    {
        //                                        //we will append a flag against the message type only if the condition is of type "" 
        //                                        //and so in this way this will help the user to resend the 270 eligiblity request again for this patient
        //                                        oClsRxH_271Master.MessageType = oClsRxH_271Master.MessageType + "|NCP";
        //                                    }

        //                                    clsRxGeneral.UpdateLogForRx("The request is valid, however the transaction has been rejected. No contract with payer");
        //                                }
        //                                else if (oSegment.get_DataElementValue(3) == "42")//GSE=General system error
        //                                {
        //                                    if (oClsRxH_271Master.MessageType != "")
        //                                    {
        //                                        //we will append a flag against the message type only if the condition is of type "" 
        //                                        //and so in this way this will help the user to resend the 270 eligiblity request again for this patient
        //                                        oClsRxH_271Master.MessageType = oClsRxH_271Master.MessageType + "|GSE";
        //                                    }

        //                                    clsRxGeneral.UpdateLogForRx("The request is valid, however the transaction has been rejected. General system error");
        //                                }

        //                            }
        //                            else
        //                            {
        //                                //MessageBox.Show("The request or an element in the request is not valid. The transaction has been rejected", "gloRxhub", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //                                clsRxGeneral.UpdateLogForRx("The request or an element in the request is not valid. The transaction has been rejected");
        //                                break;
        //                                sValue.Append(oSegment.get_DataElementValue(2) + Environment.NewLine);
        //                                if (oSegment.get_DataElementValue(3).Trim() != "")
        //                                {
        //                                    //listResponse.Items.Add("Payer Rejection Reason: " + GetSubscriberRejectionReason(oSegment.get_DataElementValue(3)));
        //                                    //listResponse.Items.Add("Payer Follow up: " + GetSubscriberFollowUp(oSegment.get_DataElementValue(4)));
        //                                }

        //                                EDIReturnResult = oSegment.get_DataElementValue(3).Trim() + "-" + oSegment.get_DataElementValue(4).Trim();

        //                            }
        //                        }
        //                        else if (sSegmentID == "DMG")
        //                        {
        //                            sValue.Append("Subscriber Demographic Information : " + oSegment.get_DataElementValue(1) + Environment.NewLine);
        //                            sValue.Append("Subscriber Date of Birth : " + oSegment.get_DataElementValue(2) + Environment.NewLine);
        //                            //txtSubscriberDOB.Text = oSegment.get_DataElementValue(2);//Subscriber Date of birth
        //                            oClsRxH_271Details.SubscriberDOB = oSegment.get_DataElementValue(2);//Subscriber Date of birth

        //                            sValue.Append("Subscriber Gender : " + oSegment.get_DataElementValue(3) + Environment.NewLine);
        //                            //txtSubscriberGender.Text = oSegment.get_DataElementValue(3);//Subscriber Gender
        //                            oClsRxH_271Details.SubscriberGender = oSegment.get_DataElementValue(3);//Subscriber Gender

        //                        }
        //                        else if (sSegmentID == "INS")
        //                        {
        //                            //If INS03 is populated with �001� and INS04 is populated with �25�, some patient demographic information returned in the 271 response differs from what was submitted in the 270 request
        //                            if (oSegment.get_DataElementValue(3) == "001" && oSegment.get_DataElementValue(4) == "25")
        //                            {
        //                                //txtIsDemographicsChanged.Text = "True";
        //                                //oClsRxH_271Master.IsDemographicsChanged = "True";
        //                                oClsRxH_271Details.IsSubscriberdemoChange = "True";
        //                                //%%%%%%%%%%%%%%%%%%%%%%%%PATIENT MATCH VERIFICATION

        //                                //*********since in oPatient.DOB we have binded the date in {7/5/1975 12:00:00 AM}	format so in 271 file generation we send 19750705(year month day) format, so to match we hv written the following code.
        //                                string _PatientDOB = "";
        //                                if (oPatient.DOB != null)
        //                                {
        //                                    _PatientDOB = oPatient.DOB.Date.ToString("yyyyMMdd");
        //                                }
        //                                //*********

        //                                //*********since in oPatient.Gender we have binded "Male", "Female", "Other" so in 271 file generation we send "M","F","O" so to match we hv written the following code.
        //                                string _PatientGender = "";
        //                                if (oPatient.Gender == "Male")
        //                                {
        //                                    _PatientGender = "M";
        //                                }
        //                                else if (oPatient.Gender == "Female")
        //                                {
        //                                    _PatientGender = "F";
        //                                }
        //                                else if (oPatient.Gender == "Other")
        //                                {
        //                                    _PatientGender = "O";
        //                                }
        //                                //*********
        //                                StringBuilder strmessage = new StringBuilder();
        //                                strmessage.Append("Patient Match Verification : The Response 271 file has different data than that of 270 Request file");
        //                                strmessage.Append(Environment.NewLine);

        //                                StringBuilder str270PatientDemographics = new StringBuilder();
        //                                str270PatientDemographics.Append("270 Request contains : " + oPatient.LastName + " " + oPatient.FirstName + " " + oPatient.MiddleName + " " + oPatient.PatientAddress.Zip + " " + _PatientDOB + " " + _PatientGender);
        //                                str270PatientDemographics.Append(Environment.NewLine);

        //                                strmessage.Append(str270PatientDemographics);

        //                                StringBuilder str271PatientDemographics = new StringBuilder();
        //                                str271PatientDemographics.Append("271 Response contains : " + oClsRxH_271Details.SubscriberLastName + " " + oClsRxH_271Details.SubscriberFirstName + " " + oClsRxH_271Details.SubscriberMiddleName + " " + oClsRxH_271Details.SubscriberZip + " " + oClsRxH_271Details.SubscriberDOB + " " + oClsRxH_271Details.SubscriberGender);
        //                                oClsRxH_271Details.SubscriberDemoChgLastName = oClsRxH_271Details.SubscriberLastName;
        //                                oClsRxH_271Details.SubscriberDemochgFirstName = oClsRxH_271Details.SubscriberFirstName;
        //                                oClsRxH_271Details.SubscriberDemoChgMiddleName = oClsRxH_271Details.SubscriberMiddleName;
        //                                oClsRxH_271Details.SubscriberDemoChgZip = oClsRxH_271Details.SubscriberZip;
        //                                oClsRxH_271Details.SubscriberDemoChgDOB = oClsRxH_271Details.SubscriberDOB;
        //                                oClsRxH_271Details.SubscriberDemoChgGender = oClsRxH_271Details.SubscriberGender;
        //                                oClsRxH_271Details.SubscriberDemoChgState = oClsRxH_271Details.SubscriberState;
        //                                oClsRxH_271Details.SubscriberDemoChgCity = oClsRxH_271Details.SubscriberCity;
        //                                oClsRxH_271Details.SubscriberDemoChgAddress1 = oClsRxH_271Details.SubscriberAddress1;
        //                                oClsRxH_271Details.SubscriberDemoChgAddress2 = oClsRxH_271Details.SubscriberAddress2;

        //                                strmessage.Append(str271PatientDemographics);

        //                                //MessageBox.Show(strmessage.ToString(), "gloRxHub", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //                                //%%%%%%%%%%%%%%%%%%%%%%%%PATIENT MATCH VERIFICATION
        //                            }
        //                            else
        //                            {
        //                                //txtIsDemographicsChanged.Text = "False";
        //                                //oClsRxH_271Master.IsDemographicsChanged = "False";
        //                                oClsRxH_271Details.IsSubscriberdemoChange = "False";
        //                            }

        //                            if (oSegment.get_DataElementValue(1) == "Y")
        //                            {
        //                                if (oSegment.get_DataElementValue(2) == "18")
        //                                {
        //                                    oClsRxH_271Master.RelationshipCode = oSegment.get_DataElementValue(2);
        //                                    oClsRxH_271Master.RelationshipDescription = "Self";
        //                                }

        //                            }
        //                            else//subscriber is dependant
        //                            {
        //                                oClsRxH_271Master.RelationshipCode = oSegment.get_DataElementValue(2);
        //                                oClsRxH_271Master.RelationshipDescription = "Dependant";
        //                            }
        //                        }
        //                        else if (sSegmentID == "DTP")
        //                        {
        //                            Qlfr = oSegment.get_DataElementValue(1);
        //                            if (Qlfr == "307")//Eligiblity Range of dates when the subscriber or dependent were eligible for benefits
        //                            {
        //                                oClsRxH_271Master.EligiblityDate = oSegment.get_DataElementValue(3);
        //                            }
        //                            else if (Qlfr == "472")//Service (Recommended by RxHub) Begin and end dates of the service being rendered
        //                            {
        //                                oClsRxH_271Master.ServiceDate = oSegment.get_DataElementValue(3);
        //                            }
        //                            sValue.Append("Subscriber Service : " + oSegment.get_DataElementValue(1) + Environment.NewLine);
        //                            sValue.Append("Subscriber Date : " + oSegment.get_DataElementValue(2) + Environment.NewLine);
        //                            sValue.Append("Subscriber Service Date : " + oSegment.get_DataElementValue(3) + Environment.NewLine);

        //                        }
        //                    }

        //                    else if (sLoopSection == "HL;NM1;EB")
        //                    {

        //                        if (sSegmentID == "EB")
        //                        {
        //                            Qlfr = oSegment.get_DataElementValue(1);
        //                        }
        //                        if (sSegmentID == "EB")
        //                        {
        //                            if (blnNM1Found == false)
        //                            {
        //                                //Only if the Qlfr =1 tht means pahrmacy has active coverage for claim then make the IsPharmacy flag to true,
        //                                //else keep it false
        //                                #region " Subscriber EB Segment (When NM1 is IL) "
        //                                if (Qlfr == "1")//Active Coverage
        //                                {
        //                                    //listResponse.Items.Add("Active Coverage: " + oSegment.get_DataElementValue(3).Trim());
        //                                    if (oSegment.get_DataElementValue(3) == "88")
        //                                    {
        //                                        oClsRxH_271Master.IsPharmacyEligible = "Yes";

        //                                        #region "Insurance type code - Retail"
        //                                        Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
        //                                        if (Qlfr_InsuranceTypeCode != "")
        //                                        {
        //                                            if (Qlfr_InsuranceTypeCode == "47")
        //                                            {
        //                                                oClsRxH_271Master.RetailInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
        //                                            }
        //                                            else if (Qlfr_InsuranceTypeCode == "CP")
        //                                            {
        //                                                oClsRxH_271Master.RetailInsTypeCode = "Medicare Conditionally Primary";
        //                                            }
        //                                            else if (Qlfr_InsuranceTypeCode == "MC")
        //                                            {
        //                                                oClsRxH_271Master.RetailInsTypeCode = "Medicaid";
        //                                            }
        //                                            else if (Qlfr_InsuranceTypeCode == "MP")
        //                                            {
        //                                                oClsRxH_271Master.RetailInsTypeCode = "Medicare Primary";
        //                                            }
        //                                            else if (Qlfr_InsuranceTypeCode == "OT")
        //                                            {
        //                                                oClsRxH_271Master.RetailInsTypeCode = "Other (Used for Medicare Part D)";
        //                                            }

        //                                        }
        //                                        #endregion "Insurance type code - Retail"

        //                                        //oClsRxH_271Master.RetailInsTypeCode = oSegment.get_DataElementValue(4);
        //                                        oClsRxH_271Master.PharmacyCoveragePlanName = oSegment.get_DataElementValue(5);
        //                                        oClsRxH_271Master.RetailMonetaryAmount = oSegment.get_DataElementValue(7);
        //                                        oClsRxH_271Master.PharmacyEligiblityorBenefitInfo = "Active Coverage";
        //                                        IsPharmacy = true;

        //                                    }
        //                                    if (oSegment.get_DataElementValue(3) == "90")
        //                                    {
        //                                        oClsRxH_271Master.IsMailOrdRxDrugEligible = "Yes";
        //                                        #region "Insurance type code - Mail"
        //                                        Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
        //                                        if (Qlfr_InsuranceTypeCode != "")
        //                                        {
        //                                            if (Qlfr_InsuranceTypeCode == "47")
        //                                            {
        //                                                oClsRxH_271Master.MailOrderInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
        //                                            }
        //                                            else if (Qlfr_InsuranceTypeCode == "CP")
        //                                            {
        //                                                oClsRxH_271Master.MailOrderInsTypeCode = "Medicare Conditionally Primary";
        //                                            }
        //                                            else if (Qlfr_InsuranceTypeCode == "MC")
        //                                            {
        //                                                oClsRxH_271Master.MailOrderInsTypeCode = "Medicaid";
        //                                            }
        //                                            else if (Qlfr_InsuranceTypeCode == "MP")
        //                                            {
        //                                                oClsRxH_271Master.MailOrderInsTypeCode = "Medicare Primary";
        //                                            }
        //                                            else if (Qlfr_InsuranceTypeCode == "OT")
        //                                            {
        //                                                oClsRxH_271Master.MailOrderInsTypeCode = "Other (Used for Medicare Part D)";
        //                                            }

        //                                        }
        //                                        #endregion "Insurance type code - Mail"
        //                                        //oClsRxH_271Master.MailOrderInsTypeCode = oSegment.get_DataElementValue(4);
        //                                        oClsRxH_271Master.MailOrdRxDrugCoveragePlanName = oSegment.get_DataElementValue(5);
        //                                        oClsRxH_271Master.MailOrderMonetaryAmount = oSegment.get_DataElementValue(7);
        //                                        oClsRxH_271Master.MailOrdRxDrugEligiblityorBenefitInfo = "Active Coverage";
        //                                        IsMailOrdRx = true;

        //                                    }
        //                                }

        //                                else if (Qlfr == "6")//Inactive
        //                                {
        //                                    //listResponse.Items.Add("Co-Payment: " + oSegment.get_DataElementValue(1).Trim());
        //                                    if (oSegment.get_DataElementValue(3).Trim() == "88")
        //                                    {
        //                                        oClsRxH_271Master.IsPharmacyEligible = "NO";
        //                                        #region "Insurance type code - Retail"
        //                                        Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
        //                                        if (Qlfr_InsuranceTypeCode != "")
        //                                        {
        //                                            if (Qlfr_InsuranceTypeCode == "47")
        //                                            {
        //                                                oClsRxH_271Master.RetailInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
        //                                            }
        //                                            else if (Qlfr_InsuranceTypeCode == "CP")
        //                                            {
        //                                                oClsRxH_271Master.RetailInsTypeCode = "Medicare Conditionally Primary";
        //                                            }
        //                                            else if (Qlfr_InsuranceTypeCode == "MC")
        //                                            {
        //                                                oClsRxH_271Master.RetailInsTypeCode = "Medicaid";
        //                                            }
        //                                            else if (Qlfr_InsuranceTypeCode == "MP")
        //                                            {
        //                                                oClsRxH_271Master.RetailInsTypeCode = "Medicare Primary";
        //                                            }
        //                                            else if (Qlfr_InsuranceTypeCode == "OT")
        //                                            {
        //                                                oClsRxH_271Master.RetailInsTypeCode = "Other (Used for Medicare Part D)";
        //                                            }

        //                                        }
        //                                        #endregion "Insurance type code - Retail"
        //                                        //oClsRxH_271Master.RetailInsTypeCode = oSegment.get_DataElementValue(4);
        //                                        //oClsRxH_271Master.PharmacyCoveragePlanName = oSegment.get_DataElementValue(5);
        //                                        oClsRxH_271Master.RetailMonetaryAmount = oSegment.get_DataElementValue(7);
        //                                        oClsRxH_271Master.PharmacyEligiblityorBenefitInfo = "Inactive";
        //                                        IsPharmacy = true;

        //                                    }
        //                                    if (oSegment.get_DataElementValue(3) == "90")
        //                                    {
        //                                        oClsRxH_271Master.IsMailOrdRxDrugEligible = "NO";
        //                                        #region "Insurance type code - Mail"
        //                                        Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
        //                                        if (Qlfr_InsuranceTypeCode != "")
        //                                        {
        //                                            if (Qlfr_InsuranceTypeCode == "47")
        //                                            {
        //                                                oClsRxH_271Master.MailOrderInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
        //                                            }
        //                                            else if (Qlfr_InsuranceTypeCode == "CP")
        //                                            {
        //                                                oClsRxH_271Master.MailOrderInsTypeCode = "Medicare Conditionally Primary";
        //                                            }
        //                                            else if (Qlfr_InsuranceTypeCode == "MC")
        //                                            {
        //                                                oClsRxH_271Master.MailOrderInsTypeCode = "Medicaid";
        //                                            }
        //                                            else if (Qlfr_InsuranceTypeCode == "MP")
        //                                            {
        //                                                oClsRxH_271Master.MailOrderInsTypeCode = "Medicare Primary";
        //                                            }
        //                                            else if (Qlfr_InsuranceTypeCode == "OT")
        //                                            {
        //                                                oClsRxH_271Master.MailOrderInsTypeCode = "Other (Used for Medicare Part D)";
        //                                            }

        //                                        }
        //                                        #endregion "Insurance type code - Mail"
        //                                        //oClsRxH_271Master.MailOrderInsTypeCode = oSegment.get_DataElementValue(4);
        //                                        // oClsRxH_271Master.MailOrdRxDrugCoveragePlanName = oSegment.get_DataElementValue(5);
        //                                        oClsRxH_271Master.MailOrdRxDrugEligiblityorBenefitInfo = "Inactive";
        //                                        oClsRxH_271Master.MailOrderMonetaryAmount = oSegment.get_DataElementValue(7);
        //                                        IsMailOrdRx = true;

        //                                    }
        //                                }
        //                                else if (Qlfr == "G")//Out Of Pocket (Stop Loss)
        //                                {
        //                                    //listResponse.Items.Add("Deductibles: " + oSegment.get_DataElementValue(1).Trim());
        //                                    if (oSegment.get_DataElementValue(3).Trim() == "88")
        //                                    {
        //                                        oClsRxH_271Master.IsPharmacyEligible = "NO";
        //                                        #region "Insurance type code - Retail"
        //                                        Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
        //                                        if (Qlfr_InsuranceTypeCode != "")
        //                                        {
        //                                            if (Qlfr_InsuranceTypeCode == "47")
        //                                            {
        //                                                oClsRxH_271Master.RetailInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
        //                                            }
        //                                            else if (Qlfr_InsuranceTypeCode == "CP")
        //                                            {
        //                                                oClsRxH_271Master.RetailInsTypeCode = "Medicare Conditionally Primary";
        //                                            }
        //                                            else if (Qlfr_InsuranceTypeCode == "MC")
        //                                            {
        //                                                oClsRxH_271Master.RetailInsTypeCode = "Medicaid";
        //                                            }
        //                                            else if (Qlfr_InsuranceTypeCode == "MP")
        //                                            {
        //                                                oClsRxH_271Master.RetailInsTypeCode = "Medicare Primary";
        //                                            }
        //                                            else if (Qlfr_InsuranceTypeCode == "OT")
        //                                            {
        //                                                oClsRxH_271Master.RetailInsTypeCode = "Other (Used for Medicare Part D)";
        //                                            }

        //                                        }
        //                                        #endregion "Insurance type code - Retail"
        //                                        //oClsRxH_271Master.RetailInsTypeCode = oSegment.get_DataElementValue(4);
        //                                        //oClsRxH_271Master.PharmacyCoveragePlanName = oSegment.get_DataElementValue(5);
        //                                        oClsRxH_271Master.PharmacyEligiblityorBenefitInfo = "Out of Pocket (Stop Loss)";
        //                                        oClsRxH_271Master.RetailMonetaryAmount = oSegment.get_DataElementValue(7);
        //                                        IsPharmacy = true;

        //                                    }
        //                                    if (oSegment.get_DataElementValue(3) == "90")
        //                                    {
        //                                        oClsRxH_271Master.IsMailOrdRxDrugEligible = "NO";
        //                                        #region "Insurance type code - Mail"
        //                                        Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
        //                                        if (Qlfr_InsuranceTypeCode != "")
        //                                        {
        //                                            if (Qlfr_InsuranceTypeCode == "47")
        //                                            {
        //                                                oClsRxH_271Master.MailOrderInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
        //                                            }
        //                                            else if (Qlfr_InsuranceTypeCode == "CP")
        //                                            {
        //                                                oClsRxH_271Master.MailOrderInsTypeCode = "Medicare Conditionally Primary";
        //                                            }
        //                                            else if (Qlfr_InsuranceTypeCode == "MC")
        //                                            {
        //                                                oClsRxH_271Master.MailOrderInsTypeCode = "Medicaid";
        //                                            }
        //                                            else if (Qlfr_InsuranceTypeCode == "MP")
        //                                            {
        //                                                oClsRxH_271Master.MailOrderInsTypeCode = "Medicare Primary";
        //                                            }
        //                                            else if (Qlfr_InsuranceTypeCode == "OT")
        //                                            {
        //                                                oClsRxH_271Master.MailOrderInsTypeCode = "Other (Used for Medicare Part D)";
        //                                            }

        //                                        }
        //                                        #endregion "Insurance type code - Mail"
        //                                        // oClsRxH_271Master.MailOrderInsTypeCode = oSegment.get_DataElementValue(4);
        //                                        // oClsRxH_271Master.MailOrdRxDrugCoveragePlanName = oSegment.get_DataElementValue(5);
        //                                        oClsRxH_271Master.MailOrdRxDrugEligiblityorBenefitInfo = "Out of Pocket (Stop Loss)";
        //                                        oClsRxH_271Master.MailOrderMonetaryAmount = oSegment.get_DataElementValue(7);
        //                                        IsMailOrdRx = true;

        //                                    }
        //                                }
        //                                else if (Qlfr == "V")//Cannot Process
        //                                {
        //                                    //listResponse.Items.Add("Deductibles: " + oSegment.get_DataElementValue(1).Trim());
        //                                    if (oSegment.get_DataElementValue(3).Trim() == "88")
        //                                    {
        //                                        oClsRxH_271Master.IsPharmacyEligible = "NO";
        //                                        #region "Insurance type code - Retail"
        //                                        Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
        //                                        if (Qlfr_InsuranceTypeCode != "")
        //                                        {
        //                                            if (Qlfr_InsuranceTypeCode == "47")
        //                                            {
        //                                                oClsRxH_271Master.RetailInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
        //                                            }
        //                                            else if (Qlfr_InsuranceTypeCode == "CP")
        //                                            {
        //                                                oClsRxH_271Master.RetailInsTypeCode = "Medicare Conditionally Primary";
        //                                            }
        //                                            else if (Qlfr_InsuranceTypeCode == "MC")
        //                                            {
        //                                                oClsRxH_271Master.RetailInsTypeCode = "Medicaid";
        //                                            }
        //                                            else if (Qlfr_InsuranceTypeCode == "MP")
        //                                            {
        //                                                oClsRxH_271Master.RetailInsTypeCode = "Medicare Primary";
        //                                            }
        //                                            else if (Qlfr_InsuranceTypeCode == "OT")
        //                                            {
        //                                                oClsRxH_271Master.RetailInsTypeCode = "Other (Used for Medicare Part D)";
        //                                            }

        //                                        }
        //                                        #endregion "Insurance type code - Retail"
        //                                        //oClsRxH_271Master.RetailInsTypeCode = oSegment.get_DataElementValue(4);
        //                                        //oClsRxH_271Master.PharmacyCoveragePlanName = oSegment.get_DataElementValue(5);
        //                                        oClsRxH_271Master.PharmacyEligiblityorBenefitInfo = "Cannot Process";
        //                                        oClsRxH_271Master.RetailMonetaryAmount = oSegment.get_DataElementValue(7);
        //                                        IsPharmacy = true;

        //                                    }
        //                                    if (oSegment.get_DataElementValue(3) == "90")
        //                                    {
        //                                        oClsRxH_271Master.IsMailOrdRxDrugEligible = "NO";
        //                                        #region "Insurance type code - Mail"
        //                                        Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
        //                                        if (Qlfr_InsuranceTypeCode != "")
        //                                        {
        //                                            if (Qlfr_InsuranceTypeCode == "47")
        //                                            {
        //                                                oClsRxH_271Master.MailOrderInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
        //                                            }
        //                                            else if (Qlfr_InsuranceTypeCode == "CP")
        //                                            {
        //                                                oClsRxH_271Master.MailOrderInsTypeCode = "Medicare Conditionally Primary";
        //                                            }
        //                                            else if (Qlfr_InsuranceTypeCode == "MC")
        //                                            {
        //                                                oClsRxH_271Master.MailOrderInsTypeCode = "Medicaid";
        //                                            }
        //                                            else if (Qlfr_InsuranceTypeCode == "MP")
        //                                            {
        //                                                oClsRxH_271Master.MailOrderInsTypeCode = "Medicare Primary";
        //                                            }
        //                                            else if (Qlfr_InsuranceTypeCode == "OT")
        //                                            {
        //                                                oClsRxH_271Master.MailOrderInsTypeCode = "Other (Used for Medicare Part D)";
        //                                            }

        //                                        }
        //                                        #endregion "Insurance type code - Mail"
        //                                        //oClsRxH_271Master.MailOrderInsTypeCode = oSegment.get_DataElementValue(4);
        //                                        // oClsRxH_271Master.MailOrdRxDrugCoveragePlanName = oSegment.get_DataElementValue(5);
        //                                        oClsRxH_271Master.MailOrdRxDrugEligiblityorBenefitInfo = "Cannot Process";
        //                                        oClsRxH_271Master.MailOrderMonetaryAmount = oSegment.get_DataElementValue(7);
        //                                        IsMailOrdRx = true;

        //                                    }
        //                                }


        //                                #endregion " Subscriber EB Segment (When NM1 is IL) "
        //                            }//IF loop end for Subscriber EB Segment
        //                            else
        //                            {
        //                                if (blnContractedProvider == true)
        //                                {
        //                                    #region " Contracted Provider EB Segment (When NM1 is 13) "
        //                                    if (Qlfr == "1")//Active Coverage
        //                                    {
        //                                        //listResponse.Items.Add("Active Coverage: " + oSegment.get_DataElementValue(3).Trim());
        //                                        if (oSegment.get_DataElementValue(3) == "88")
        //                                        {
        //                                            oClsRxH_271Master.ContProvRetailsEligible = "Yes";
        //                                            oClsRxH_271Master.ContProvRetailCoverageInfo = "Active Coverage";
        //                                            //Mail ord insurance type code value
        //                                            #region "Insurance type code - Retail"
        //                                            Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
        //                                            if (Qlfr_InsuranceTypeCode != "")
        //                                            {
        //                                                if (Qlfr_InsuranceTypeCode == "47")
        //                                                {
        //                                                    oClsRxH_271Master.ContProvRetailInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "CP")
        //                                                {
        //                                                    oClsRxH_271Master.ContProvRetailInsTypeCode = "Medicare Conditionally Primary";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "MC")
        //                                                {
        //                                                    oClsRxH_271Master.ContProvRetailInsTypeCode = "Medicaid";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "MP")
        //                                                {
        //                                                    oClsRxH_271Master.ContProvRetailInsTypeCode = "Medicare Primary";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "OT")
        //                                                {
        //                                                    oClsRxH_271Master.ContProvRetailInsTypeCode = "Other (Used for Medicare Part D)";
        //                                                }

        //                                            }
        //                                            #endregion "Insurance type code - Retail"
        //                                            // oClsRxH_271Master.ContProvRetailInsTypeCode = oSegment.get_DataElementValue(4);
        //                                            //Mail ord Monetary amount
        //                                            oClsRxH_271Master.ContProvRetailMonetaryAmt = oSegment.get_DataElementValue(7);

        //                                        }
        //                                        if (oSegment.get_DataElementValue(3) == "90")
        //                                        {
        //                                            oClsRxH_271Master.ContProvMailOrderEligible = "Yes";
        //                                            oClsRxH_271Master.ContProvMailOrderCoverageInfo = "Active Coverage";
        //                                            //Mail ord insurance type code value
        //                                            #region "Insurance type code - Mail"
        //                                            Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
        //                                            if (Qlfr_InsuranceTypeCode != "")
        //                                            {
        //                                                if (Qlfr_InsuranceTypeCode == "47")
        //                                                {
        //                                                    oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "CP")
        //                                                {
        //                                                    oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Medicare Conditionally Primary";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "MC")
        //                                                {
        //                                                    oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Medicaid";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "MP")
        //                                                {
        //                                                    oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Medicare Primary";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "OT")
        //                                                {
        //                                                    oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Other (Used for Medicare Part D)";
        //                                                }

        //                                            }
        //                                            #endregion "Insurance type code - Mail"
        //                                            //oClsRxH_271Master.ContProvMailOrderInsTypeCode = oSegment.get_DataElementValue(4);
        //                                            //Mail ord Monetary amount
        //                                            oClsRxH_271Master.ContProvMailOrderMonetaryAmt = oSegment.get_DataElementValue(7);
        //                                        }
        //                                    }

        //                                    else if (Qlfr == "6")//Inactive
        //                                    {
        //                                        //listResponse.Items.Add("Co-Payment: " + oSegment.get_DataElementValue(1).Trim());
        //                                        if (oSegment.get_DataElementValue(3).Trim() == "88")
        //                                        {

        //                                            oClsRxH_271Master.ContProvRetailsEligible = "NO";
        //                                            oClsRxH_271Master.ContProvRetailCoverageInfo = "Inactive";
        //                                            //Mail ord insurance type code value
        //                                            #region "Insurance type code - Retail"
        //                                            Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
        //                                            if (Qlfr_InsuranceTypeCode != "")
        //                                            {
        //                                                if (Qlfr_InsuranceTypeCode == "47")
        //                                                {
        //                                                    oClsRxH_271Master.ContProvRetailInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "CP")
        //                                                {
        //                                                    oClsRxH_271Master.ContProvRetailInsTypeCode = "Medicare Conditionally Primary";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "MC")
        //                                                {
        //                                                    oClsRxH_271Master.ContProvRetailInsTypeCode = "Medicaid";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "MP")
        //                                                {
        //                                                    oClsRxH_271Master.ContProvRetailInsTypeCode = "Medicare Primary";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "OT")
        //                                                {
        //                                                    oClsRxH_271Master.ContProvRetailInsTypeCode = "Other (Used for Medicare Part D)";
        //                                                }

        //                                            }
        //                                            #endregion "Insurance type code - Retail"
        //                                            //oClsRxH_271Master.ContProvRetailInsTypeCode = oSegment.get_DataElementValue(4);
        //                                            //Mail ord Monetary amount
        //                                            oClsRxH_271Master.ContProvRetailMonetaryAmt = oSegment.get_DataElementValue(7);
        //                                        }
        //                                        if (oSegment.get_DataElementValue(3) == "90")
        //                                        {
        //                                            oClsRxH_271Master.ContProvMailOrderEligible = "NO";
        //                                            oClsRxH_271Master.ContProvMailOrderCoverageInfo = "Inactive";
        //                                            //Mail ord insurance type code value
        //                                            #region "Insurance type code - Mail"
        //                                            Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
        //                                            if (Qlfr_InsuranceTypeCode != "")
        //                                            {
        //                                                if (Qlfr_InsuranceTypeCode == "47")
        //                                                {
        //                                                    oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "CP")
        //                                                {
        //                                                    oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Medicare Conditionally Primary";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "MC")
        //                                                {
        //                                                    oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Medicaid";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "MP")
        //                                                {
        //                                                    oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Medicare Primary";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "OT")
        //                                                {
        //                                                    oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Other (Used for Medicare Part D)";
        //                                                }

        //                                            }
        //                                            #endregion "Insurance type code - Mail"
        //                                            //oClsRxH_271Master.ContProvMailOrderInsTypeCode = oSegment.get_DataElementValue(4);
        //                                            //Mail ord Monetary amount
        //                                            oClsRxH_271Master.ContProvMailOrderMonetaryAmt = oSegment.get_DataElementValue(7);

        //                                        }
        //                                    }
        //                                    else if (Qlfr == "G")//Out Of Pocket (Stop Loss)
        //                                    {
        //                                        //listResponse.Items.Add("Deductibles: " + oSegment.get_DataElementValue(1).Trim());
        //                                        if (oSegment.get_DataElementValue(3).Trim() == "88")
        //                                        {

        //                                            oClsRxH_271Master.ContProvRetailsEligible = "NO";
        //                                            oClsRxH_271Master.ContProvRetailCoverageInfo = "Out of Pocket (Stop Loss)";
        //                                            //Mail ord insurance type code value
        //                                            #region "Insurance type code - Retail"
        //                                            Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
        //                                            if (Qlfr_InsuranceTypeCode != "")
        //                                            {
        //                                                if (Qlfr_InsuranceTypeCode == "47")
        //                                                {
        //                                                    oClsRxH_271Master.ContProvRetailInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "CP")
        //                                                {
        //                                                    oClsRxH_271Master.ContProvRetailInsTypeCode = "Medicare Conditionally Primary";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "MC")
        //                                                {
        //                                                    oClsRxH_271Master.ContProvRetailInsTypeCode = "Medicaid";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "MP")
        //                                                {
        //                                                    oClsRxH_271Master.ContProvRetailInsTypeCode = "Medicare Primary";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "OT")
        //                                                {
        //                                                    oClsRxH_271Master.ContProvRetailInsTypeCode = "Other (Used for Medicare Part D)";
        //                                                }

        //                                            }
        //                                            #endregion "Insurance type code - Retail"
        //                                            //oClsRxH_271Master.ContProvRetailInsTypeCode = oSegment.get_DataElementValue(4);
        //                                            //Mail ord Monetary amount
        //                                            oClsRxH_271Master.ContProvRetailMonetaryAmt = oSegment.get_DataElementValue(7);
        //                                        }
        //                                        if (oSegment.get_DataElementValue(3) == "90")
        //                                        {

        //                                            oClsRxH_271Master.ContProvMailOrderEligible = "NO";
        //                                            oClsRxH_271Master.ContProvMailOrderCoverageInfo = "Out of Pocket (Stop Loss)";
        //                                            //Mail ord insurance type code value
        //                                            #region "Insurance type code - Mail"
        //                                            Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
        //                                            if (Qlfr_InsuranceTypeCode != "")
        //                                            {
        //                                                if (Qlfr_InsuranceTypeCode == "47")
        //                                                {
        //                                                    oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "CP")
        //                                                {
        //                                                    oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Medicare Conditionally Primary";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "MC")
        //                                                {
        //                                                    oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Medicaid";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "MP")
        //                                                {
        //                                                    oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Medicare Primary";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "OT")
        //                                                {
        //                                                    oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Other (Used for Medicare Part D)";
        //                                                }

        //                                            }
        //                                            #endregion "Insurance type code - Mail"
        //                                            //oClsRxH_271Master.ContProvMailOrderInsTypeCode = oSegment.get_DataElementValue(4);
        //                                            //Mail ord Monetary amount
        //                                            oClsRxH_271Master.ContProvMailOrderMonetaryAmt = oSegment.get_DataElementValue(7);

        //                                        }
        //                                    }
        //                                    else if (Qlfr == "V")//Cannot Process
        //                                    {
        //                                        //listResponse.Items.Add("Deductibles: " + oSegment.get_DataElementValue(1).Trim());
        //                                        if (oSegment.get_DataElementValue(3).Trim() == "88")
        //                                        {
        //                                            oClsRxH_271Master.ContProvRetailsEligible = "NO";
        //                                            oClsRxH_271Master.ContProvRetailCoverageInfo = "Cannot Process";
        //                                            //Mail ord insurance type code value
        //                                            #region "Insurance type code - Retail"
        //                                            Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
        //                                            if (Qlfr_InsuranceTypeCode != "")
        //                                            {
        //                                                if (Qlfr_InsuranceTypeCode == "47")
        //                                                {
        //                                                    oClsRxH_271Master.ContProvRetailInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "CP")
        //                                                {
        //                                                    oClsRxH_271Master.ContProvRetailInsTypeCode = "Medicare Conditionally Primary";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "MC")
        //                                                {
        //                                                    oClsRxH_271Master.ContProvRetailInsTypeCode = "Medicaid";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "MP")
        //                                                {
        //                                                    oClsRxH_271Master.ContProvRetailInsTypeCode = "Medicare Primary";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "OT")
        //                                                {
        //                                                    oClsRxH_271Master.ContProvRetailInsTypeCode = "Other (Used for Medicare Part D)";
        //                                                }

        //                                            }
        //                                            #endregion "Insurance type code - Retail"
        //                                            //oClsRxH_271Master.ContProvRetailInsTypeCode = oSegment.get_DataElementValue(4);
        //                                            //Mail ord Monetary amount
        //                                            oClsRxH_271Master.ContProvRetailMonetaryAmt = oSegment.get_DataElementValue(7);
        //                                        }
        //                                        if (oSegment.get_DataElementValue(3) == "90")
        //                                        {
        //                                            oClsRxH_271Master.ContProvMailOrderEligible = "NO";
        //                                            oClsRxH_271Master.ContProvMailOrderCoverageInfo = "Cannot Process";
        //                                            //Mail ord insurance type code value
        //                                            #region "Insurance type code - Mail"
        //                                            Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
        //                                            if (Qlfr_InsuranceTypeCode != "")
        //                                            {
        //                                                if (Qlfr_InsuranceTypeCode == "47")
        //                                                {
        //                                                    oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "CP")
        //                                                {
        //                                                    oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Medicare Conditionally Primary";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "MC")
        //                                                {
        //                                                    oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Medicaid";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "MP")
        //                                                {
        //                                                    oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Medicare Primary";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "OT")
        //                                                {
        //                                                    oClsRxH_271Master.ContProvMailOrderInsTypeCode = "Other (Used for Medicare Part D)";
        //                                                }

        //                                            }
        //                                            #endregion "Insurance type code - Mail"
        //                                            //oClsRxH_271Master.ContProvMailOrderInsTypeCode = oSegment.get_DataElementValue(4);
        //                                            //Mail ord Monetary amount
        //                                            oClsRxH_271Master.ContProvMailOrderMonetaryAmt = oSegment.get_DataElementValue(7);

        //                                        }
        //                                    }
        //                                    #endregion " Contracted Provider EB Segment (When NM1 is 13) "
        //                                }// IF loop end for Contracted provider EB Segment
        //                                else if (blnPrimaryPayer == true)
        //                                {
        //                                    #region " Primary Payer EB Segment (When NM1 is PRP) "
        //                                    if (Qlfr == "1")//Active Coverage
        //                                    {
        //                                        //listResponse.Items.Add("Active Coverage: " + oSegment.get_DataElementValue(3).Trim());
        //                                        if (oSegment.get_DataElementValue(3) == "88")
        //                                        {

        //                                            oClsRxH_271Master.PrimaryPayerRetailsEligible = "Yes";
        //                                            oClsRxH_271Master.PrimaryPayerRetailCoverageInfo = "Active Coverage";
        //                                            //oClsRxH_271Master.MailOrdRxDrugCoveragePlanName = oSegment.get_DataElementValue(5);
        //                                            #region "Insurance type code - Retail"
        //                                            Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
        //                                            if (Qlfr_InsuranceTypeCode != "")
        //                                            {
        //                                                if (Qlfr_InsuranceTypeCode == "47")
        //                                                {
        //                                                    oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "CP")
        //                                                {
        //                                                    oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Medicare Conditionally Primary";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "MC")
        //                                                {
        //                                                    oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Medicaid";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "MP")
        //                                                {
        //                                                    oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Medicare Primary";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "OT")
        //                                                {
        //                                                    oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Other (Used for Medicare Part D)";
        //                                                }

        //                                            }
        //                                            #endregion "Insurance type code - Retail"
        //                                            //oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = oSegment.get_DataElementValue(4);
        //                                            oClsRxH_271Master.PrimaryPayerRetailMonetaryAmt = oSegment.get_DataElementValue(7);

        //                                        }
        //                                        if (oSegment.get_DataElementValue(3) == "90")
        //                                        {
        //                                            oClsRxH_271Master.PrimaryPayerMailOrderEligible = "Yes";
        //                                            oClsRxH_271Master.PrimaryPayerMailOrderCoverageInfo = "Active Coverage";
        //                                            #region "Insurance type code - Mail"
        //                                            Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
        //                                            if (Qlfr_InsuranceTypeCode != "")
        //                                            {
        //                                                if (Qlfr_InsuranceTypeCode == "47")
        //                                                {
        //                                                    oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "CP")
        //                                                {
        //                                                    oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Medicare Conditionally Primary";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "MC")
        //                                                {
        //                                                    oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Medicaid";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "MP")
        //                                                {
        //                                                    oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Medicare Primary";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "OT")
        //                                                {
        //                                                    oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Other (Used for Medicare Part D)";
        //                                                }

        //                                            }
        //                                            #endregion "Insurance type code - Mail"
        //                                            //oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = oSegment.get_DataElementValue(4);
        //                                            oClsRxH_271Master.PrimaryPayerMailOrderMonetaryAmt = oSegment.get_DataElementValue(7);

        //                                        }
        //                                    }

        //                                    else if (Qlfr == "6")//Inactive
        //                                    {
        //                                        //listResponse.Items.Add("Co-Payment: " + oSegment.get_DataElementValue(1).Trim());
        //                                        if (oSegment.get_DataElementValue(3).Trim() == "88")
        //                                        {
        //                                            oClsRxH_271Master.PrimaryPayerRetailsEligible = "NO";
        //                                            oClsRxH_271Master.PrimaryPayerRetailCoverageInfo = "Inactive";
        //                                            //oClsRxH_271Master.MailOrdRxDrugCoveragePlanName = oSegment.get_DataElementValue(5);
        //                                            #region "Insurance type code - Retail"
        //                                            Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
        //                                            if (Qlfr_InsuranceTypeCode != "")
        //                                            {
        //                                                if (Qlfr_InsuranceTypeCode == "47")
        //                                                {
        //                                                    oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "CP")
        //                                                {
        //                                                    oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Medicare Conditionally Primary";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "MC")
        //                                                {
        //                                                    oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Medicaid";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "MP")
        //                                                {
        //                                                    oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Medicare Primary";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "OT")
        //                                                {
        //                                                    oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Other (Used for Medicare Part D)";
        //                                                }

        //                                            }
        //                                            #endregion "Insurance type code - Retail"
        //                                            //oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = oSegment.get_DataElementValue(4);
        //                                            oClsRxH_271Master.PrimaryPayerRetailMonetaryAmt = oSegment.get_DataElementValue(7);

        //                                        }
        //                                        if (oSegment.get_DataElementValue(3) == "90")
        //                                        {
        //                                            oClsRxH_271Master.PrimaryPayerMailOrderEligible = "NO";
        //                                            oClsRxH_271Master.PrimaryPayerMailOrderCoverageInfo = "Inactive";
        //                                            #region "Insurance type code - Mail"
        //                                            Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
        //                                            if (Qlfr_InsuranceTypeCode != "")
        //                                            {
        //                                                if (Qlfr_InsuranceTypeCode == "47")
        //                                                {
        //                                                    oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "CP")
        //                                                {
        //                                                    oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Medicare Conditionally Primary";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "MC")
        //                                                {
        //                                                    oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Medicaid";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "MP")
        //                                                {
        //                                                    oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Medicare Primary";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "OT")
        //                                                {
        //                                                    oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Other (Used for Medicare Part D)";
        //                                                }

        //                                            }
        //                                            #endregion "Insurance type code - Mail"
        //                                            oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = oSegment.get_DataElementValue(4);
        //                                            oClsRxH_271Master.PrimaryPayerMailOrderMonetaryAmt = oSegment.get_DataElementValue(7);

        //                                        }
        //                                    }
        //                                    else if (Qlfr == "G")//Out Of Pocket (Stop Loss)
        //                                    {
        //                                        //listResponse.Items.Add("Deductibles: " + oSegment.get_DataElementValue(1).Trim());
        //                                        if (oSegment.get_DataElementValue(3).Trim() == "88")
        //                                        {
        //                                            oClsRxH_271Master.PrimaryPayerRetailsEligible = "NO";
        //                                            oClsRxH_271Master.PrimaryPayerRetailCoverageInfo = "Out of Pocket (Stop Loss)";
        //                                            //oClsRxH_271Master.MailOrdRxDrugCoveragePlanName = oSegment.get_DataElementValue(5);
        //                                            #region "Insurance type code - Retail"
        //                                            Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
        //                                            if (Qlfr_InsuranceTypeCode != "")
        //                                            {
        //                                                if (Qlfr_InsuranceTypeCode == "47")
        //                                                {
        //                                                    oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "CP")
        //                                                {
        //                                                    oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Medicare Conditionally Primary";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "MC")
        //                                                {
        //                                                    oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Medicaid";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "MP")
        //                                                {
        //                                                    oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Medicare Primary";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "OT")
        //                                                {
        //                                                    oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Other (Used for Medicare Part D)";
        //                                                }

        //                                            }
        //                                            #endregion "Insurance type code - Retail"
        //                                            //oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = oSegment.get_DataElementValue(4);
        //                                            oClsRxH_271Master.PrimaryPayerRetailMonetaryAmt = oSegment.get_DataElementValue(7);

        //                                        }
        //                                        if (oSegment.get_DataElementValue(3) == "90")
        //                                        {
        //                                            oClsRxH_271Master.PrimaryPayerMailOrderEligible = "NO";
        //                                            oClsRxH_271Master.PrimaryPayerMailOrderCoverageInfo = "Out of Pocket (Stop Loss)";
        //                                            #region "Insurance type code - Mail"
        //                                            Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
        //                                            if (Qlfr_InsuranceTypeCode != "")
        //                                            {
        //                                                if (Qlfr_InsuranceTypeCode == "47")
        //                                                {
        //                                                    oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "CP")
        //                                                {
        //                                                    oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Medicare Conditionally Primary";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "MC")
        //                                                {
        //                                                    oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Medicaid";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "MP")
        //                                                {
        //                                                    oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Medicare Primary";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "OT")
        //                                                {
        //                                                    oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Other (Used for Medicare Part D)";
        //                                                }

        //                                            }
        //                                            #endregion "Insurance type code - Mail"
        //                                            //oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = oSegment.get_DataElementValue(4);
        //                                            oClsRxH_271Master.PrimaryPayerMailOrderMonetaryAmt = oSegment.get_DataElementValue(7);

        //                                        }
        //                                    }
        //                                    else if (Qlfr == "V")//Cannot Process
        //                                    {
        //                                        //listResponse.Items.Add("Deductibles: " + oSegment.get_DataElementValue(1).Trim());
        //                                        if (oSegment.get_DataElementValue(3).Trim() == "88")
        //                                        {
        //                                            oClsRxH_271Master.PrimaryPayerRetailsEligible = "NO";
        //                                            oClsRxH_271Master.PrimaryPayerRetailCoverageInfo = "Cannot Process";
        //                                            //oClsRxH_271Master.MailOrdRxDrugCoveragePlanName = oSegment.get_DataElementValue(5);
        //                                            #region "Insurance type code - Retail"
        //                                            Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
        //                                            if (Qlfr_InsuranceTypeCode != "")
        //                                            {
        //                                                if (Qlfr_InsuranceTypeCode == "47")
        //                                                {
        //                                                    oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "CP")
        //                                                {
        //                                                    oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Medicare Conditionally Primary";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "MC")
        //                                                {
        //                                                    oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Medicaid";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "MP")
        //                                                {
        //                                                    oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Medicare Primary";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "OT")
        //                                                {
        //                                                    oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = "Other (Used for Medicare Part D)";
        //                                                }

        //                                            }
        //                                            #endregion "Insurance type code - Retail"
        //                                            //oClsRxH_271Master.PrimaryPayerRetailInsTypeCode = oSegment.get_DataElementValue(4);
        //                                            oClsRxH_271Master.PrimaryPayerRetailMonetaryAmt = oSegment.get_DataElementValue(7);

        //                                        }
        //                                        if (oSegment.get_DataElementValue(3) == "90")
        //                                        {
        //                                            oClsRxH_271Master.PrimaryPayerMailOrderEligible = "NO";
        //                                            oClsRxH_271Master.PrimaryPayerMailOrderCoverageInfo = "Cannot Process";
        //                                            #region "Insurance type code - Mail"
        //                                            Qlfr_InsuranceTypeCode = oSegment.get_DataElementValue(4);
        //                                            if (Qlfr_InsuranceTypeCode != "")
        //                                            {
        //                                                if (Qlfr_InsuranceTypeCode == "47")
        //                                                {
        //                                                    oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Medicare Secondary, Other Liability Insurance is Primary";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "CP")
        //                                                {
        //                                                    oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Medicare Conditionally Primary";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "MC")
        //                                                {
        //                                                    oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Medicaid";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "MP")
        //                                                {
        //                                                    oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Medicare Primary";
        //                                                }
        //                                                else if (Qlfr_InsuranceTypeCode == "OT")
        //                                                {
        //                                                    oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = "Other (Used for Medicare Part D)";
        //                                                }

        //                                            }
        //                                            #endregion "Insurance type code - Mail"
        //                                            //oClsRxH_271Master.PrimaryPayerMailOrderInsTypeCode = oSegment.get_DataElementValue(4);
        //                                            oClsRxH_271Master.PrimaryPayerMailOrderMonetaryAmt = oSegment.get_DataElementValue(7);

        //                                        }
        //                                    }
        //                                    #endregion " PRP EB Segment"
        //                                }//IF Loop end for Primary Payer EB Segment


        //                            }



        //                        }
        //                        else if (sSegmentID == "AAA")
        //                        {
        //                            sValue.Append(oSegment.get_DataElementValue(2) + Environment.NewLine);
        //                            if (oSegment.get_DataElementValue(3).Trim() != "")
        //                            {
        //                                //listResponse.Items.Add("Eligibility Rejection Reason: " + GetSubscriberRejectionReason(oSegment.get_DataElementValue(3)));
        //                                //listResponse.Items.Add("Eligibility Follow up: " + GetSubscriberFollowUp(oSegment.get_DataElementValue(4)));
        //                            }

        //                            EDIReturnResult = oSegment.get_DataElementValue(3).Trim() + "-" + oSegment.get_DataElementValue(4).Trim();
        //                            //AddNote(_PatientId, EDIReturnResult);
        //                        }
        //                        else if (sSegmentID == "REF")
        //                        {

        //                        }

        //                    }
        //                    else if (sLoopSection == "HL;NM1;EB;III")
        //                    {
        //                        if (sSegmentID == "III")
        //                        {

        //                        }
        //                        if (sSegmentID == "NM1")
        //                        {

        //                        }
        //                    }

        //                    else if (sLoopSection == "HL;NM1;EB;NM1")
        //                    {
        //                        if (sSegmentID == "NM1")
        //                        {
        //                            blnNM1Found = true;
        //                        }

        //                        //read the Contracted Provider and Primary Payer information in this section
        //                        if (sSegmentID == "NM1")
        //                        {
        //                            //the segment value 2 will return the value 13/PRP. if 13 insert the isContracted value as Yes/NO 
        //                            //if the value is PRP make the  isPrimaryPayer value as Yes/NO

        //                            //Contracted Provider
        //                            if (oSegment.get_DataElementValue(1) == "13")
        //                            {
        //                                blnContractedProvider = true;
        //                                oClsRxH_271Master.IsContractedProvider = "Yes";
        //                                //read the Name 
        //                                oClsRxH_271Master.ContractedProviderName = oSegment.get_DataElementValue(3);
        //                                //Number
        //                                oClsRxH_271Master.ContractedProviderNumber = oSegment.get_DataElementValue(9);

        //                            }
        //                            //else
        //                            //{
        //                            //    oClsRxH_271Master.IsContractedProvider = "No";
        //                            //}

        //                            //Primary Payer
        //                            else if (oSegment.get_DataElementValue(1) == "PRP")
        //                            {
        //                                blnPrimaryPayer = true;
        //                                blnContractedProvider = false;
        //                                oClsRxH_271Master.IsPrimaryPayer = "Yes";
        //                                //read the Name 
        //                                oClsRxH_271Master.PrimaryPayerName = oSegment.get_DataElementValue(3);
        //                                //Number
        //                                oClsRxH_271Master.PrimaryPayerNumber = oSegment.get_DataElementValue(9);

        //                                //read the EB loop present in this section


        //                            }
        //                            //else
        //                            //{
        //                            //    oClsRxH_271Master.IsPrimaryPayer = "No";
        //                            //}

        //                        }


        //                    }

        //                    else if (sLoopSection == "HL;NM1;EB;LS;NM1")
        //                    {
        //                        if (sSegmentID == "NM1")
        //                        {
        //                            blnNM1Found = true;
        //                        }

        //                        //read the Contracted Provider and Primary Payer information in this section
        //                        if (sSegmentID == "NM1")
        //                        {
        //                            //the segment value 2 will return the value 13/PRP. if 13 insert the isContracted value as Yes/NO 
        //                            //if the value is PRP make the  isPrimaryPayer value as Yes/NO

        //                            //Contracted Provider
        //                            if (oSegment.get_DataElementValue(1) == "13")
        //                            {
        //                                blnContractedProvider = true;
        //                                oClsRxH_271Master.IsContractedProvider = "Yes";
        //                                //read the Name 
        //                                oClsRxH_271Master.ContractedProviderName = oSegment.get_DataElementValue(3);
        //                                //Number
        //                                oClsRxH_271Master.ContractedProviderNumber = oSegment.get_DataElementValue(9);

        //                            }
        //                            //else
        //                            //{
        //                            //    oClsRxH_271Master.IsContractedProvider = "No";
        //                            //}

        //                            //Primary Payer
        //                            else if (oSegment.get_DataElementValue(1) == "PRP")
        //                            {
        //                                blnPrimaryPayer = true;
        //                                blnContractedProvider = false;
        //                                oClsRxH_271Master.IsPrimaryPayer = "Yes";
        //                                //read the Name 
        //                                oClsRxH_271Master.PrimaryPayerName = oSegment.get_DataElementValue(3);
        //                                //Number
        //                                oClsRxH_271Master.PrimaryPayerNumber = oSegment.get_DataElementValue(9);

        //                                //read the EB loop present in this section


        //                            }
        //                            //else
        //                            //{
        //                            //    oClsRxH_271Master.IsPrimaryPayer = "No";
        //                            //}

        //                        }


        //                    }
                              
        //                }
        //                //*************************** Depandant Loop
        //                else if (sEntity == "23")
        //                {
        //                    if (sLoopSection == "HL")
        //                    {
        //                        if (sSegmentID == "HL")
        //                        {
        //                            sValue.Append(oSegment.get_DataElementValue(1) + Environment.NewLine);
        //                            sValue.Append(oSegment.get_DataElementValue(2) + Environment.NewLine);
        //                            sValue.Append(oSegment.get_DataElementValue(3) + Environment.NewLine);
        //                        }
        //                        else if (sSegmentID == "TRN")
        //                        {
        //                            sValue.Append(oSegment.get_DataElementValue(1) + Environment.NewLine);
        //                            sValue.Append(oSegment.get_DataElementValue(2) + Environment.NewLine);
        //                            sValue.Append(oSegment.get_DataElementValue(3) + Environment.NewLine);
        //                        }
        //                    }

        //                    else if (sLoopSection == "HL;NM1")
        //                    {
        //                        if (sSegmentID == "NM1")
        //                        {

        //                            //sValue.Append("Subscriber Last Name : " + oSegment.get_DataElementValue(3) + Environment.NewLine);
        //                            //sValue.Append("Subscriber First Name : " + oSegment.get_DataElementValue(4) + Environment.NewLine);
        //                            //sValue.Append("Subscriber ID: " + oSegment.get_DataElementValue(9) + Environment.NewLine);
        //                            //txtDependantLastName.Text = oSegment.get_DataElementValue(3);//Dependant Last Name
        //                            oClsRxH_271Master.DLastName = oSegment.get_DataElementValue(3);//Dependant Last Name
        //                            //txtDependantFirstName.Text = oSegment.get_DataElementValue(4);//Dependant First Name
        //                            oClsRxH_271Master.DFirstName = oSegment.get_DataElementValue(4);//Dependant First Name
        //                            //txtDependantMiddleName.Text = oSegment.get_DataElementValue(5);//Dependant Middle Name
        //                            oClsRxH_271Master.DMiddleName = oSegment.get_DataElementValue(5);//Dependant Middle Name

        //                        }
        //                        else if (sSegmentID == "N3")
        //                        {
        //                            //sValue.Append("Subscriber Address : " + oSegment.get_DataElementValue(1) + Environment.NewLine);
        //                            //txtDependantAddressLine1.Text = oSegment.get_DataElementValue(1);//Dependant Address line1
        //                            oClsRxH_271Master.DAddress1 = oSegment.get_DataElementValue(1);//Dependant Address line1
        //                            //txtDependantAddressLine2.Text = oSegment.get_DataElementValue(2);//Dependant Address line2
        //                            oClsRxH_271Master.DAddress2 = oSegment.get_DataElementValue(2);//Dependant Address line2

        //                        }
        //                        else if (sSegmentID == "N4")
        //                        {
        //                            //sValue.Append("Subscriber City : " + oSegment.get_DataElementValue(1) + Environment.NewLine);
        //                            //sValue.Append("Subscriber State : " + oSegment.get_DataElementValue(2) + Environment.NewLine);
        //                            //sValue.Append("Subscriber Zip : " + oSegment.get_DataElementValue(3) + Environment.NewLine);
        //                            //txtDependantCity.Text = oSegment.get_DataElementValue(1);//Dependant City
        //                            oClsRxH_271Master.DCity = oSegment.get_DataElementValue(1);//Dependant City
        //                            //txtDependantState.Text = oSegment.get_DataElementValue(2);//Dependant State
        //                            oClsRxH_271Master.DState = oSegment.get_DataElementValue(2);//Dependant State
        //                            //txtDependantZip.Text = oSegment.get_DataElementValue(3);//Dependant Zip
        //                            oClsRxH_271Master.DZip = oSegment.get_DataElementValue(3);//Dependant Zip
        //                        }
        //                        else if (sSegmentID == "PER")
        //                        {

        //                        }
        //                        else if (sSegmentID == "REF")
        //                        {
        //                            Qlfr = oSegment.get_DataElementValue(1);
        //                            //returned Employee ID
        //                            if (Qlfr == "A6")
        //                            {
        //                                sEmployeeId = oSegment.get_DataElementValue(2);
        //                                sValue.Append("Employee ID : " + sEmployeeId + Environment.NewLine);
        //                                oClsRxH_271Master.EmployeeId = oSegment.get_DataElementValue(2);
        //                            }
        //                            //If returned, health plan name in the REF03 must be displayed in the end user application per RxHub application guideline 3.6.
        //                            if (Qlfr == "18")
        //                            {
        //                                sPlanNumber = oSegment.get_DataElementValue(2);
        //                                sValue.Append("Plan Number : " + sPlanNumber + Environment.NewLine);
        //                                //txtHealthPlanName.Text = oSegment.get_DataElementValue(3);//++++++++++++++++++++
        //                                oClsRxH_271Master.HealthPlanNumber = oSegment.get_DataElementValue(2);
        //                                oClsRxH_271Master.HealthPlanName = oSegment.get_DataElementValue(3);//++++++++++++++++++++

        //                            }
        //                            //If returned, cardholder ID in the REF02 and cardholder name in the REF03 will be used to populate drug history request and new prescription transactions.
        //                            else if (Qlfr == "1W")
        //                            {
        //                                sMemberID = oSegment.get_DataElementValue(2);
        //                                sValue.Append("Member ID : " + sMemberID + Environment.NewLine);
        //                                //txtCardHolderId.Text = oSegment.get_DataElementValue(2);//++++++++++++++++++++
        //                                oClsRxH_271Master.CardHolderId = oSegment.get_DataElementValue(2);//++++++++++++++++++++
        //                                //txtCardHolderName.Text = oSegment.get_DataElementValue(2);//++++++++++++++++++++
        //                                oClsRxH_271Master.CardHolderName = oSegment.get_DataElementValue(3);//++++++++++++++++++++

        //                            }
        //                            //If returned, group ID in the REF02 will be used to populate drug history request and new prescription transactions.
        //                            else if (Qlfr == "6P")
        //                            {

        //                                sGroupNumber = oSegment.get_DataElementValue(2);
        //                                sValue.Append("Group Number : " + sGroupNumber + Environment.NewLine);
        //                                //txtGroupId.Text = oSegment.get_DataElementValue(2);//++++++++++++++++++++
        //                                oClsRxH_271Master.GroupId = oSegment.get_DataElementValue(2);//group ID in the REF02 will be used to populate drug history request and new prescription transactions.
        //                                oClsRxH_271Master.GroupName = oSegment.get_DataElementValue(3);//group ID in the REF02 will be used to populate drug history request and new prescription transactions.
        //                            }
        //                            //If returned, formulary list ID in the REF02 and alternative list ID in the REF03 will be used to link to patient formulary and benefit data.
        //                            else if (Qlfr == "IF")
        //                            {
        //                                sFormularlyID = oSegment.get_DataElementValue(2);
        //                                sValue.Append("Formulary ID : " + sFormularlyID + Environment.NewLine);
        //                                //txtFormularyListId.Text = oSegment.get_DataElementValue(2);//++++++++++++++++++++
        //                                oClsRxH_271Master.FormularyListId = oSegment.get_DataElementValue(2);//++++++++++++++++++++
        //                                sAlternativeID = oSegment.get_DataElementValue(3);
        //                                sValue.Append("Alternative ID : " + sAlternativeID + Environment.NewLine);
        //                                //txtAlternativeListId.Text = oSegment.get_DataElementValue(3);//++++++++++++++++++++
        //                                oClsRxH_271Master.AlternativeListId = oSegment.get_DataElementValue(3);//++++++++++++++++++++

        //                            }
        //                            //If returned, coverage ID in the REF03 will be used to link to patient formulary and benefit data.
        //                            else if (Qlfr == "1L")
        //                            {
        //                                //sFormularlyID = oSegment.get_DataElementValue(2);
        //                                //sValue.Append("Formulary ID : " + sFormularlyID + Environment.NewLine);
        //                                //txtCoverageId.Text = oSegment.get_DataElementValue(3);//++++++++++++++++++++
        //                                oClsRxH_271Master.CoverageId = oSegment.get_DataElementValue(3);//++++++++++++++++++++

        //                            }
        //                            //If returned, BIN number in the REF02 will be used to populate new prescription transactions.
        //                            else if (Qlfr == "N6")
        //                            {
        //                                sBIN = oSegment.get_DataElementValue(2);
        //                                sValue.Append("BIN : " + sBIN + Environment.NewLine);
        //                                //txtBinNumber.Text = oSegment.get_DataElementValue(2);//++++++++++++++++++++
        //                                oClsRxH_271Master.BINNumber = oSegment.get_DataElementValue(2);//++++++++++++++++++++
        //                                //sPCN = oSegment.get_DataElementValue(3);
        //                                //sValue.Append("PCN : " + sPCN + Environment.NewLine);
        //                            }
        //                            //If returned, copay ID in the REF03 will be used to link to patient formulary and benefit data.
        //                            else if (Qlfr == "IG")
        //                            {
        //                                //sBIN = oSegment.get_DataElementValue(2);
        //                                //sValue.Append("BIN : " + sBIN + Environment.NewLine);
        //                                //txtCopayId.Text = oSegment.get_DataElementValue(3);//++++++++++++++++++++
        //                                oClsRxH_271Master.CopayId = oSegment.get_DataElementValue(3);//++++++++++++++++++++
        //                                //sPCN = oSegment.get_DataElementValue(3);
        //                                //sValue.Append("PCN : " + sPCN + Environment.NewLine);
        //                            }

        //                        }
        //                        else if (sSegmentID == "AAA")
        //                        {
        //                            if (oSegment.get_DataElementValue(1) == "N")
        //                            {
        //                                sValue.Append(oSegment.get_DataElementValue(2) + Environment.NewLine);
        //                                if (oSegment.get_DataElementValue(3).Trim() != "")
        //                                {
        //                                    //listResponse.Items.Add("Payer Rejection Reason: " + GetSubscriberRejectionReason(oSegment.get_DataElementValue(3)));
        //                                    //listResponse.Items.Add("Payer Follow up: " + GetSubscriberFollowUp(oSegment.get_DataElementValue(4)));
        //                                }

        //                                EDIReturnResult = oSegment.get_DataElementValue(3).Trim() + "-" + oSegment.get_DataElementValue(4).Trim();
        //                                //AddNote(_PatientId, EDIReturnResult);
        //                                //txtSubscriberInfo.Text = GetSubscriberRejectionReason(oSegment.get_DataElementValue(3));
        //                                //txtSubscriberResponse.Text = GetSubscriberRejectionReason(oSegment.get_DataElementValue(4));
        //                                //EDIReturnResult = EDIReturnResult + txtSubscriberInfo.Text.Trim() + "-" + txtSubscriberResponse.Text.Trim();
        //                                //AddNote(_PatientId, txtSubscriberInfo.Text.Trim() + "-" + txtSubscriberResponse.Text.Trim());
        //                            }
        //                        }
        //                        else if (sSegmentID == "DMG")
        //                        {
        //                            sValue.Append("Subscriber Demographic Information : " + oSegment.get_DataElementValue(1) + Environment.NewLine);
        //                            sValue.Append("Subscriber Date of Birth : " + oSegment.get_DataElementValue(2) + Environment.NewLine);
        //                            //txtDependantDOB.Text = oSegment.get_DataElementValue(2);//Dependant Date of birth
        //                            oClsRxH_271Master.DDOB = oSegment.get_DataElementValue(2);//Dependant Date of birth
        //                            sValue.Append("Subscriber Gender : " + oSegment.get_DataElementValue(3) + Environment.NewLine);
        //                            //txtDependantGender.Text = oSegment.get_DataElementValue(3);//Dependant Gender
        //                            oClsRxH_271Master.DGender = oSegment.get_DataElementValue(3);//Dependant Gender
        //                        }
        //                        else if (sSegmentID == "INS")
        //                        {
        //                            //If INS03 is populated with �001� and INS04 is populated with �25�, some patient demographic information returned in the 271 response differs from what was submitted in the 270 request
        //                            if (oSegment.get_DataElementValue(3) == "001" && oSegment.get_DataElementValue(4) == "25")
        //                            {
        //                                //txtIsDemographicsChanged.Text = "True";
        //                                oClsRxH_271Master.IsDependentdemoChange = "True";
        //                                //%%%%%%%%%%%%%%%%%%%%%%%%PATIENT MATCH VERIFICATION

        //                                //*********since in oPatient.DOB we have binded the date in {7/5/1975 12:00:00 AM}	format so in 271 file generation we send 19750705(year month day) format, so to match we hv written the following code.
        //                                string _PatientDOB = "";
        //                                if (oPatient.DOB != null)
        //                                {
        //                                    _PatientDOB = oPatient.DOB.Date.ToString("yyyyMMdd");
        //                                }
        //                                //*********

        //                                //*********since in oPatient.Gender we have binded "Male", "Female", "Other" so in 271 file generation we send "M","F","O" so to match we hv written the following code.
        //                                string _PatientGender = "";
        //                                if (oPatient.Gender == "Male")
        //                                {
        //                                    _PatientGender = "M";
        //                                }
        //                                else if (oPatient.Gender == "Female")
        //                                {
        //                                    _PatientGender = "F";
        //                                }
        //                                else if (oPatient.Gender == "Other")
        //                                {
        //                                    _PatientGender = "O";
        //                                }
        //                                //*********
        //                                StringBuilder strmessage = new StringBuilder();
        //                                strmessage.Append("Patient Match Verification : The Response 271 file has different data than that of 270 Request file");
        //                                strmessage.Append(Environment.NewLine);

        //                                StringBuilder str270PatientDemographics = new StringBuilder();
        //                                str270PatientDemographics.Append("270 Request contains : " + oPatient.LastName + " " + oPatient.FirstName + " " + oPatient.MiddleName + " " + oPatient.PatientAddress.Zip + " " + _PatientDOB + " " + _PatientGender);
        //                                str270PatientDemographics.Append(Environment.NewLine);

        //                                strmessage.Append(str270PatientDemographics);

        //                                StringBuilder str271PatientDemographics = new StringBuilder();
        //                                str271PatientDemographics.Append("271 Response contains : " + oClsRxH_271Master.DLastName + " " + oClsRxH_271Master.DFirstName + " " + oClsRxH_271Master.DMiddleName + " " + oClsRxH_271Master.DZip + " " + oClsRxH_271Master.DDOB + " " + oClsRxH_271Master.DGender);
        //                                oClsRxH_271Master.DependentdemoChgLastName = oClsRxH_271Master.DLastName;
        //                                oClsRxH_271Master.DependentdemochgFirstName = oClsRxH_271Master.DFirstName;
        //                                oClsRxH_271Master.DependentdemoChgMiddleName = oClsRxH_271Master.DMiddleName;
        //                                oClsRxH_271Master.DependentdemoChgZip = oClsRxH_271Master.DZip;
        //                                oClsRxH_271Master.DependentdemoChgDOB = oClsRxH_271Master.DDOB;
        //                                oClsRxH_271Master.DependentdemoChgGender = oClsRxH_271Master.DGender;
        //                                oClsRxH_271Master.DependentdemoChgState = oClsRxH_271Master.DState;
        //                                oClsRxH_271Master.DependentdemoChgCity = oClsRxH_271Master.DCity;
        //                                oClsRxH_271Master.DependentdemoChgSSN = oClsRxH_271Master.DSSN;
        //                                oClsRxH_271Master.DependentdemoChgAddress1 = oClsRxH_271Master.DAddress1;
        //                                oClsRxH_271Master.DependentdemoChgAddress2 = oClsRxH_271Master.DAddress2;

        //                                strmessage.Append(str271PatientDemographics);

        //                                //MessageBox.Show(strmessage.ToString(), "gloRxHub", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //                                //%%%%%%%%%%%%%%%%%%%%%%%%PATIENT MATCH VERIFICATION
        //                            }
        //                            else
        //                            {
        //                                //txtIsDemographicsChanged.Text = "False";
        //                                oClsRxH_271Master.IsDependentdemoChange = "False";
        //                            }

        //                            if (oSegment.get_DataElementValue(1) == "Y")
        //                            {
        //                                if (oSegment.get_DataElementValue(2) == "18")
        //                                {
        //                                    oClsRxH_271Master.RelationshipCode = oSegment.get_DataElementValue(2);
        //                                    oClsRxH_271Master.RelationshipDescription = "Self";
        //                                }

        //                            }
        //                            else//subscriber is dependant
        //                            {
        //                                oClsRxH_271Master.RelationshipCode = oSegment.get_DataElementValue(2);
        //                                oClsRxH_271Master.RelationshipDescription = "Dependant";
        //                            }
        //                        }
        //                        else if (sSegmentID == "DTP")
        //                        {
        //                            Qlfr = oSegment.get_DataElementValue(1);
        //                            if (Qlfr == "307")//Eligiblity Range of dates when the subscriber or dependent were eligible for benefits
        //                            {
        //                                oClsRxH_271Master.EligiblityDate = oSegment.get_DataElementValue(3);
        //                            }
        //                            else if (Qlfr == "472")//Service (Recommended by RxHub) Begin and end dates of the service being rendered
        //                            {
        //                                oClsRxH_271Master.ServiceDate = oSegment.get_DataElementValue(3);
        //                            }
        //                            //sValue.Append("Subscriber Service : " + oSegment.get_DataElementValue(1) + Environment.NewLine);
        //                            //sValue.Append("Subscriber Date : " + oSegment.get_DataElementValue(2) + Environment.NewLine);
        //                            //sValue.Append("Subscriber Service Date : " + oSegment.get_DataElementValue(3) + Environment.NewLine);
        //                        }
        //                    }

        //                    else if (sLoopSection == "HL;NM1;EB")
        //                    {
        //                        if (sSegmentID == "EB")
        //                        {
        //                            Qlfr = oSegment.get_DataElementValue(1);
        //                        }
        //                        if (sSegmentID == "EB")
        //                        {
        //                            //Only if the Qlfr =1 tht means pahrmacy has active coverage for claim then make the IsPharmacy flag to true,
        //                            //else keep it false
        //                            #region Commented code
        //                            //if (Qlfr == "1")//Active Coverage
        //                            //{
        //                            //    //listResponse.Items.Add("Active Coverage: " + oSegment.get_DataElementValue(3).Trim());
        //                            //    if (oSegment.get_DataElementValue(3) == "88")
        //                            //    {
        //                            //        IsPharmacy = true;
        //                            //        sValue.Append("Pharmacy : " + "1" + Environment.NewLine);
        //                            //    }
        //                            //    if (oSegment.get_DataElementValue(3) == "90")
        //                            //    {
        //                            //        IsMailOrdRx = true;
        //                            //        sValue.Append("Mail Order Prescription : " + "1" + Environment.NewLine);
        //                            //    }
        //                            //}
        //                            //else if (Qlfr == "6")//Inactive
        //                            //{
        //                            //    //listResponse.Items.Add("Co-Payment: " + oSegment.get_DataElementValue(1).Trim());
        //                            //    if (oSegment.get_DataElementValue(3).Trim() == "88")
        //                            //    {
        //                            //        IsPharmacy = false;
        //                            //        sValue.Append("Pharmacy : " + "6" + Environment.NewLine);
        //                            //    }
        //                            //    if (oSegment.get_DataElementValue(3) == "90")
        //                            //    {
        //                            //        IsMailOrdRx = false;
        //                            //        sValue.Append("Mail Order Prescription : " + "6" + Environment.NewLine);
        //                            //    }
        //                            //}
        //                            //else if (Qlfr == "G")//Out Of Pocket (Stop Loss)
        //                            //{
        //                            //    //listResponse.Items.Add("Deductibles: " + oSegment.get_DataElementValue(1).Trim());
        //                            //    if (oSegment.get_DataElementValue(3).Trim() == "88")
        //                            //    {
        //                            //        IsPharmacy = false;
        //                            //        sValue.Append("Pharmacy : " + "G" + Environment.NewLine);
        //                            //    }
        //                            //    if (oSegment.get_DataElementValue(3) == "90")
        //                            //    {
        //                            //        IsMailOrdRx = false;
        //                            //        sValue.Append("Mail Order Prescription : " + "G" + Environment.NewLine);
        //                            //    }
        //                            //}
        //                            //else if (Qlfr == "V")//Cannot Process
        //                            //{
        //                            //    //listResponse.Items.Add("Deductibles: " + oSegment.get_DataElementValue(1).Trim());
        //                            //    if (oSegment.get_DataElementValue(3).Trim() == "88")
        //                            //    {
        //                            //        IsPharmacy = false;
        //                            //        sValue.Append("Pharmacy : " + "V" + Environment.NewLine);
        //                            //    }
        //                            //    if (oSegment.get_DataElementValue(3) == "90")
        //                            //    {
        //                            //        IsMailOrdRx = false;
        //                            //        sValue.Append("Mail Order Prescription : " + "V" + Environment.NewLine);
        //                            //    }
        //                            //}
        //                            #endregion

        //                            if (blnNM1Found == false)
        //                            {
        //                                //Only if the Qlfr =1 tht means pahrmacy has active coverage for claim then make the IsPharmacy flag to true,
        //                                //else keep it false

        //                                if (Qlfr == "1")//Active Coverage
        //                                {
        //                                    //listResponse.Items.Add("Active Coverage: " + oSegment.get_DataElementValue(3).Trim());
        //                                    if (oSegment.get_DataElementValue(3) == "88")
        //                                    {
        //                                        oClsRxH_271Master.IsPharmacyEligible = "Yes";
        //                                        oClsRxH_271Master.PharmacyCoveragePlanName = oSegment.get_DataElementValue(5);
        //                                        oClsRxH_271Master.PharmacyEligiblityorBenefitInfo = "Active Coverage";
        //                                        IsPharmacy = true;
        //                                        sValue.Append("Pharmacy : " + "1" + Environment.NewLine);
        //                                    }
        //                                    if (oSegment.get_DataElementValue(3) == "90")
        //                                    {
        //                                        oClsRxH_271Master.IsMailOrdRxDrugEligible = "Yes";
        //                                        oClsRxH_271Master.MailOrdRxDrugCoveragePlanName = oSegment.get_DataElementValue(5);
        //                                        oClsRxH_271Master.MailOrdRxDrugEligiblityorBenefitInfo = "Active Coverage";
        //                                        IsMailOrdRx = true;
        //                                        sValue.Append("Mail Order Prescription : " + "1" + Environment.NewLine);
        //                                    }
        //                                }

        //                                else if (Qlfr == "6")//Inactive
        //                                {
        //                                    //listResponse.Items.Add("Co-Payment: " + oSegment.get_DataElementValue(1).Trim());
        //                                    if (oSegment.get_DataElementValue(3).Trim() == "88")
        //                                    {
        //                                        oClsRxH_271Master.IsPharmacyEligible = "NO";
        //                                        //oClsRxH_271Master.PharmacyCoveragePlanName = oSegment.get_DataElementValue(5);
        //                                        oClsRxH_271Master.PharmacyEligiblityorBenefitInfo = "Inactive";
        //                                        IsPharmacy = true;
        //                                        sValue.Append("Pharmacy : " + "1" + Environment.NewLine);
        //                                    }
        //                                    if (oSegment.get_DataElementValue(3) == "90")
        //                                    {
        //                                        oClsRxH_271Master.IsMailOrdRxDrugEligible = "NO";
        //                                        // oClsRxH_271Master.MailOrdRxDrugCoveragePlanName = oSegment.get_DataElementValue(5);
        //                                        oClsRxH_271Master.MailOrdRxDrugEligiblityorBenefitInfo = "Inactive";
        //                                        IsMailOrdRx = true;
        //                                        sValue.Append("Mail Order Prescription : " + "1" + Environment.NewLine);
        //                                    }
        //                                }
        //                                else if (Qlfr == "G")//Out Of Pocket (Stop Loss)
        //                                {
        //                                    //listResponse.Items.Add("Deductibles: " + oSegment.get_DataElementValue(1).Trim());
        //                                    if (oSegment.get_DataElementValue(3).Trim() == "88")
        //                                    {
        //                                        oClsRxH_271Master.IsPharmacyEligible = "NO";
        //                                        //oClsRxH_271Master.PharmacyCoveragePlanName = oSegment.get_DataElementValue(5);
        //                                        oClsRxH_271Master.PharmacyEligiblityorBenefitInfo = "Out of Pocket (Stop Loss)";
        //                                        IsPharmacy = true;
        //                                        sValue.Append("Pharmacy : " + "1" + Environment.NewLine);
        //                                    }
        //                                    if (oSegment.get_DataElementValue(3) == "90")
        //                                    {
        //                                        oClsRxH_271Master.IsMailOrdRxDrugEligible = "NO";
        //                                        // oClsRxH_271Master.MailOrdRxDrugCoveragePlanName = oSegment.get_DataElementValue(5);
        //                                        oClsRxH_271Master.MailOrdRxDrugEligiblityorBenefitInfo = "Out of Pocket (Stop Loss)";
        //                                        IsMailOrdRx = true;
        //                                        sValue.Append("Mail Order Prescription : " + "1" + Environment.NewLine);
        //                                    }
        //                                }
        //                                else if (Qlfr == "V")//Cannot Process
        //                                {
        //                                    //listResponse.Items.Add("Deductibles: " + oSegment.get_DataElementValue(1).Trim());
        //                                    if (oSegment.get_DataElementValue(3).Trim() == "88")
        //                                    {
        //                                        oClsRxH_271Master.IsPharmacyEligible = "NO";
        //                                        //oClsRxH_271Master.PharmacyCoveragePlanName = oSegment.get_DataElementValue(5);
        //                                        oClsRxH_271Master.PharmacyEligiblityorBenefitInfo = "Cannot Process";
        //                                        IsPharmacy = true;
        //                                        sValue.Append("Pharmacy : " + "1" + Environment.NewLine);
        //                                    }
        //                                    if (oSegment.get_DataElementValue(3) == "90")
        //                                    {
        //                                        oClsRxH_271Master.IsMailOrdRxDrugEligible = "NO";
        //                                        // oClsRxH_271Master.MailOrdRxDrugCoveragePlanName = oSegment.get_DataElementValue(5);
        //                                        oClsRxH_271Master.MailOrdRxDrugEligiblityorBenefitInfo = "Cannot Process";
        //                                        IsMailOrdRx = true;
        //                                        sValue.Append("Mail Order Prescription : " + "1" + Environment.NewLine);
        //                                    }
        //                                }
        //                            }
        //                        }
        //                        else if (sSegmentID == "AAA")
        //                        {
        //                            sValue.Append(oSegment.get_DataElementValue(2) + Environment.NewLine);
        //                            if (oSegment.get_DataElementValue(3).Trim() != "")
        //                            {
        //                                //listResponse.Items.Add("Eligibility Rejection Reason: " + GetSubscriberRejectionReason(oSegment.get_DataElementValue(3)));
        //                                //listResponse.Items.Add("Eligibility Follow up: " + GetSubscriberFollowUp(oSegment.get_DataElementValue(4)));
        //                            }

        //                            EDIReturnResult = oSegment.get_DataElementValue(3).Trim() + "-" + oSegment.get_DataElementValue(4).Trim();
        //                            //AddNote(_PatientId, EDIReturnResult);
        //                        }
        //                        else if (sSegmentID == "REF")
        //                        {

        //                        }

        //                    }
        //                    else if (sLoopSection == "HL;NM1;EB;III")
        //                    {
        //                        if (sSegmentID == "III")
        //                        {

        //                        }
        //                    }
        //                    else if (sLoopSection == "HL;NM1;EB;NM1")
        //                    {
        //                        if (sSegmentID == "NM1")
        //                        {
        //                            blnNM1Found = true;
        //                        }
        //                    }

        //                }
        //                //***************************
        //            }
        //            ediDataSegment.Set(ref oSegment, (ediDataSegment)oSegment.Next());

        //            if (oSegment != null)
        //            {
        //                if (oSegment.ID == "SE")
        //                {
        //                    string str = oSegment.get_DataElementValue(2);
        //                    if (oClsRxH_271Master != null)
        //                    {
        //                        oClsRxH_271Master.STLoopCount = str;
        //                        oClsRxH_271Master.RxH_271Details.Add(oClsRxH_271Details);
        //                        oClsRxH_271Master.PatientId = _PatientId;
        //                        oCls271Information.Add(oClsRxH_271Master);
        //                        SaveEDIResponse271(oClsRxH_271Master);
        //                        oClsRxH_271Details = null;
        //                        oClsRxH_271Details = new ClsRxH_271Details();
        //                        oClsRxH_271Master = null;
        //                        oClsRxH_271Master = new ClsRxH_271Master();
        //                    }
        //                    blnNM1Found = false;
        //                }
        //            }

        //        }


        //        // Checks the 997 acknowledgment file just created.
        //        // The 997 file is an EDI file, so the logic to read the 997 Functional Acknowledgemnt file is similar
        //        // to translating any other EDI file.

        //        // Gets the first segment of the 997 acknowledgment file
        //        ediDataSegment.Set(ref oSegment, (ediDataSegment)oAck.GetFirst997DataSegment());	//oSegment = (ediDataSegment) oAck.GetFirst997DataSegment();

        //        while (oSegment != null)
        //        {
        //            nArea = oSegment.Area;
        //            sLoopSection = oSegment.LoopSection;
        //            sSegmentID = oSegment.ID;

        //            if (nArea == 1)
        //            {
        //                if (sLoopSection == "")
        //                {
        //                    if (sSegmentID == "AK9")
        //                    {
        //                        if (oSegment.get_DataElementValue(1, 0) == "R")
        //                        {
        //                            //MessageBox.Show("Rejected",_messageBoxCaption,MessageBoxButtons.OK,MessageBoxIcon.Information);
        //                        }
        //                    }
        //                }	// sLoopSection == ""
        //            }	//nArea == 1
        //            ediDataSegment.Set(ref oSegment, (ediDataSegment)oSegment.Next());	//oSegment = (ediDataSegment) oSegment.Next();
        //        }	//oSegment != null

        //        //save the acknowledgment
        //        string sDirectoryPath = AppDomain.CurrentDomain.BaseDirectory + "997_277\\";
        //        if (System.IO.Directory.Exists(sDirectoryPath) == false) { System.IO.Directory.CreateDirectory(sDirectoryPath); }
        //        oAck.Save(sDirectoryPath + "997_270.X12");



        //        if (oCls271Information != null)
        //        {
        //            //oClsRxH_271Master.RxH_271Details.Add(oClsRxH_271Details);

        //            return oCls271Information;// oClsRxH_271Master;
        //        }
        //        else
        //        {
        //            return null;
        //        }
        //        //if (sValue.Length > 0)
        //        //{
        //        //    //listResponse.Items.Add(sValue);
        //        //    //rchtxtbxRead271.Text = sValue.ToString();


        //        //    //insert the data to the EDIResponse271 Table
        //        //    //InsertEDIResponse271(_PatientId, "0", sSenderID, sRecieverID, sdtEligiblityDate, sMemberID, sPlanNumber, sGroupNumber, sFormularlyID, sAlternativeID, sBIN, sPCN, IsDemographicChange, IsPharmacy, IsMailOrdRx, "");

        //        //}


        //    }
        //    catch (ClsRxHubDBLayerException ex)
        //    {
        //        gloAuditTrail.gloAuditTrail.ExceptionLog(gloAuditTrail.ActivityModule.Prescription, gloAuditTrail.ActivityCategory.CreatePrescription, gloAuditTrail.ActivityType.General, ex.ToString(), gloAuditTrail.ActivityOutCome.Failure);
        //        throw ex;

        //    }
        //    catch (ClsRxHubInterfaceException ex)
        //    {
        //        gloAuditTrail.gloAuditTrail.ExceptionLog(gloAuditTrail.ActivityModule.Prescription, gloAuditTrail.ActivityCategory.CreatePrescription, gloAuditTrail.ActivityType.General, ex.ToString(), gloAuditTrail.ActivityOutCome.Failure);
        //        throw ex;

        //    }
        //    catch (Exception ex)
        //    {
        //        gloAuditTrail.gloAuditTrail.ExceptionLog(gloAuditTrail.ActivityModule.Prescription, gloAuditTrail.ActivityCategory.CreatePrescription, gloAuditTrail.ActivityType.General, ex.ToString(), gloAuditTrail.ActivityOutCome.Failure);
        //        throw ex;

        //    }
        }


    

        #endregion

        public DataSet GetMedHxRequestParameters(Int64 PatientID, string PBMName, string PBMMemberID, Int64 nLoginProviderid)
        {
            SqlConnection con = null;
            SqlCommand cmd = null;
            SqlDataAdapter adp = null;
            DataSet dtEligibilityInfo = null;
            try
            {
                con = new SqlConnection(ClsgloRxHubGeneral.ConnectionString);
                cmd = new SqlCommand("gsp_GetMedHxRequestInfo", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@nPatientID", PatientID));
                cmd.Parameters.Add(new SqlParameter("@strPBM", PBMName));
                cmd.Parameters.Add(new SqlParameter("@strPBMMemberID", PBMMemberID));
                cmd.Parameters.Add(new SqlParameter("@nLoginProviderid", nLoginProviderid));

                adp = new SqlDataAdapter(cmd);
                dtEligibilityInfo = new DataSet();
                adp.Fill(dtEligibilityInfo);

            }
            catch (Exception ex)
            {
                //gloAuditTrail.gloAuditTrail.ExceptionLog(gloAuditTrail.ActivityModule.Medication, gloAuditTrail.ActivityCategory.MedicationHistory, gloAuditTrail.ActivityType.General, ex.ToString(), gloAuditTrail.ActivityOutCome.Failure);
                throw ex;
            }

            finally
            {
                if (adp != null) { adp.Dispose(); adp = null; }
                if (cmd != null) { cmd.Dispose(); cmd = null; }
                if (con != null) { con.Dispose(); con = null; }
            }
            return dtEligibilityInfo;
        }
    }

    internal sealed class BindingFactory
    {
        private BindingFactory()
        {
        }
        static internal WSHttpBinding CreateInstance()
        {
            WSHttpBinding binding = new WSHttpBinding();
            try
            {
                binding.Security.Mode = SecurityMode.Transport;
                binding.ReliableSession.Enabled = false;
                binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
                binding.UseDefaultWebProxy = true;
                binding.OpenTimeout = new TimeSpan(0, 10, 0);
                binding.CloseTimeout = new TimeSpan(0, 10, 0);
                binding.SendTimeout = new TimeSpan(0, 10, 10);
                binding.ReceiveTimeout = new TimeSpan(0, 10, 0);
                binding.MaxBufferPoolSize = 99999999999999L;
                binding.MaxReceivedMessageSize = 2147483647;


                binding.ReaderQuotas.MaxArrayLength = 2147483647;
                binding.ReaderQuotas.MaxDepth = 64;
                binding.ReaderQuotas.MaxStringContentLength = 2147483647;

                binding.ReaderQuotas.MaxBytesPerRead = 568556652;
                binding.ReaderQuotas.MaxNameTableCharCount = 568556652;


                return (binding);
            }
            catch (Exception ex)
            {
                throw ex;
                //return binding;
            }
            finally
            {
                if ((binding != null))
                {
                    binding = null;
                }
            }
        }

    }



}