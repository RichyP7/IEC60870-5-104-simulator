﻿namespace IEC60870_5_104_simulator.Domain
{
    public enum Iec104DataTypes
    {
        ASDU_TYPEUNDEF = 0,
        M_SP_NA_1,
        M_SP_TA_1,
        M_DP_NA_1,
        M_DP_TA_1,
        M_ST_NA_1,
        M_ST_TA_1,
        M_BO_NA_1,
        M_BO_TA_1,
        M_ME_NA_1,
        M_ME_TA_1,
        M_ME_NB_1,
        M_ME_TB_1,
        M_ME_NC_1,
        M_ME_TC_1,
        M_IT_NA_1,
        M_IT_TA_1,
        M_EP_TA_1,
        M_EP_TB_1,
        M_EP_TC_1,
        M_PS_NA_1,
        M_ME_ND_1,
        ASDU_TYPE_22_29,
        M_SP_TB_1 =30,
        M_DP_TB_1,
        M_ST_TB_1,
        M_BO_TB_1,
        M_ME_TD_1,
        M_ME_TE_1,
        M_ME_TF_1,
        M_IT_TB_1,
        M_EP_TD_1,
        M_EP_TE_1,
        M_EP_TF_1,
        ASDU_TYPE_41_44,
        C_SC_NA_1 = 45,
        C_DC_NA_1,
        C_RC_NA_1,
        C_SE_NA_1,
        C_SE_NB_1,
        C_SE_NC_1,
        C_BO_NA_1,
        ASDU_TYPE_52_57,
        C_SC_TA_1 =58,
        C_DC_TA_1,
        C_RC_TA_1,
        C_SE_TA_1,
        C_SE_TB_1,
        C_SE_TC_1,
        C_BO_TA_1,
        ASDU_TYPE_65_69,
        M_EI_NA_1 =70,
        ASDU_TYPE_71_99,
        C_IC_NA_1 =80,
        C_CI_NA_1,
        C_RD_NA_1,
        C_CS_NA_1,
        C_TS_NA_1,
        C_RP_NA_1,
        C_CD_NA_1,
        C_TS_TA_1,
        ASDU_TYPE_108_109,
        P_ME_NA_1 =110,
        P_ME_NB_1,
        P_ME_NC_1,
        P_AC_NA_1,
        ASDU_TYPE_114_119,
        F_FR_NA_1 =120,
        F_SR_NA_1,
        F_SC_NA_1,
        F_LS_NA_1,
        F_FA_NA_1,
        F_SG_NA_1,
        F_DR_TA_1,
        ASDU_TYPE_127_255
    }
}