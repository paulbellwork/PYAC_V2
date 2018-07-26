using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PYAC.Infrastructure
{
    public class Hardware
    {
        public static List<String> SegmentSummaryTagsList = new List<string>()
        {
            // Zone 1 
            "Zone1_Temp",
            "Zone1_OP",
            
            // Zone 2 
            "Zone2_Temp",
            "Zone2_OP",
               
            // Zone 1 and 2
            "Zone_Cool",
            "Set_Pt",

            // Running Segment
            "Running_Rem_Segments",
            "Running_Nb",
            "Running_Soak",

            "Running_Ramp_1",
            "Running_Ramp_2",
            "Running_Ramp_3",
            "Running_Ramp_4",
            "Running_Ramp_5",
            "Running_Ramp_6",
            "Running_Ramp_7",
            "Running_Ramp_8",

            
            // Next Segment
            "Next_Nb",
           
            // TC
            "TC_Active",
            "TC_Highest",
            "TC_Lowest",
            "TC_Gbl_Average",

            // TR
            "TR_Highest",
            "TR_Lowest",

            // General Info
            "Nb_Segments",

            // Parts Temp Control
            "Parts_Alarm_Delay",
            "Parts_Min_Soak",
            "Parts_Max_Soak",
            "Parts_Low_Temp",
            "Parts_Ramp_Rate_Max",
            "Parts_Ramp_Rate_Min",
            "Parts_Temp_Thresh",
            "Parts_Unseal_Door",

            "Parts_PLC_Alarm_Delay",
            "Parts_PLC_Min_Soak",
            "Parts_PLC_Max_Soak",
            "Parts_PLC_Low_Temp",
            "Parts_PLC_Ramp_Rate_Max",
            "Parts_PLC_Ramp_Rate_Min",
            "Parts_PLC_Temp_Thresh",
            "Parts_PLC_Unseal_Door"
        };

        public static List<String> SegmentSummaryPropertiesList = new List<string>()
        {
            // Zone 1 
            "Zone1Temp",
            "Zone1OP",
            
            // Zone 2 
            "Zone2Temp",
            "Zone2OP",
               
            // Zone 1 and 2
            "ZoneCool",
            "SetPt",

            // Running Segment
            "RunningRemSegments",
            "RunningNb",
            "RunningSoak",

            "RunningRamp1",
            "RunningRamp2",
            "RunningRamp3",
            "RunningRamp4",
            "RunningRamp5",
            "RunningRamp6",
            "RunningRamp7",
            "RunningRamp8",
            
            // Next Segment
            "NextNb",
           
            // TC
            "TCActive",
            "TCHighest",
            "TCLowest",
            "TCGblAverage",

            // TR
            "TRHighest",
            "TRLowest",

            // General Info
            "NbSegments",

            // Parts Temp Control
            "PartsAlarmDelay",
            "PartsMinSoak",
            "PartsMaxSoak",
            "PartsLowTemp",
            "PartsRampRateMax",
            "PartsRampRateMin",
            "PartsTempThresh",
            "PartsUnsealDoor",

            "PartsPLCAlarmDelay",
            "PartsPLCMinSoak",
            "PartsPLCMaxSoak",
            "PartsPLCLowTemp",
            "PartsPLCRampRateMax",
            "PartsPLCRampRateMin",
            "PartsPLCTempThresh",
            "PartsPLCUnsealDoor"

        };
        public static List<String> PLCApplyChangesTagsList = new List<string>()
        {
        "Parts_Alarm_Delay",
        "Parts_Min_Soak",
        "Parts_Max_Soak",
        "Parts_Low_Temp",
        "Parts_Ramp_Rate_Max",
        "Parts_Ramp_Rate_Min",
        "Parts_Temp_Thresh",
        "Parts_Unseal_Door"
        };
        public static List<String> PLCApplyChangesPropertiesList = new List<string>()
        {
        "PartsAlarmDelay",
        "PartsMinSoak",
        "PartsMaxSoak",
        "PartsLowTemp",
        "PartsRampRateMax",
        "PartsRampRateMin",
        "PartsTempThresh",
        "PartsUnsealDoor"
        };
        public static List<String> SegmentParameterTagsList = new List<string>()
        {
            // Zone 1 
            "Zone1_Temp",
            "Zone1_OP",
            
            // Zone 2 
            "Zone2_Temp",
            "Zone2_OP",
               
            // Zone 1 and 2
            "Zone_Cool",
            "Set_Pt",

            // General Info
            "Nb_Segments"
        };
        public static List<String> SegmentParameterPropertiesList = new List<string>()
        {
            // Zone 1 
            "Zone1Temp",
            "Zone1OP",
            
            // Zone 2 
            "Zone2Temp",
            "Zone2OP",
               
            // Zone 1 and 2
            "ZoneCool",
            "SetPt",
           
            // General Info
            "NbSegments"
        };
        public static List<String> OffsetTagsList = new List<string>()
        {
            "Set_Pt",

            //Offset
            "Offset_Original_Value",
            "Offset_New_SP"
        };
        public static List<String> OffsetPropertiesList = new List<string>()
        {
            "SetPt",
            
            //Offset
            "OffsetOriginalValue",
            "OffsetNewSP"
        };
    }
}
