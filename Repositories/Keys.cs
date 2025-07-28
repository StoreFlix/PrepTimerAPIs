
namespace ServiceFabricApp.API.Repositories
{
    /// <summary>
    /// Keys
    /// </summary>
    public class Keys
    {

        #region Equipment 
        //Stored Procedure to call it from Equipment Controller
        public const string EquipmentProcedure = "SL_GetEquipmentReadonly";

        //Stored Procedure to call it from Equipment Property Controller
        public const string EquipmentPropertyProcedure = "SL_GetEquipmentPropertyReadonly";

        //Stored Procedure to call it from Equipment Alert Controller
        public const string EquipmentAlertProcedure = "SL_GetEquipmentAlertReadonly";

        //Stored Procedure to call it from Report Filter Controller
        public const string ReportFilterProcedure = "SL_GetReportFilterReadonly";

        //Stored Procedure to call it from Report Sub Filter Controller i.e, calling from the popup
        public const string ReportSubFilterProcedure = "SL_GetReportSubFilterReadonly";
        #endregion

        #region Alert
        //Stored Procedure to fetch the alerts report based on the requested generated
        public const string AlertBasicParameterFilterProcedure = "SL_GetAlertBasicParameterFilter2";

        public const string AlertBasicParameterScheduleSaveUpdateProcedure = "SL_SetAlertBasicParameterSchedule1";

        public const string AlertBasicParameterScheduleGetProcedure = "SL_GetAlertBasicParameterSchedule1";

        public const string AlertBasicParameterScheduleDeleteProcedure = "SL_DeleteAlertBasicParameterSchedule";

        //Stored Procedure to fetch the alerts report based on the requested generated
        public const string AlertReportFilterProcedure = "SL_GetAlertReportFilterReadonly";

        //Stored Procedure to fetch the alerts report based on the requested generated
        public const string AlertReportProcedure = "SL_GetAlertReportReadonly";
        #endregion

        #region Report Schedule
        //Stored Procedure to fetch the Report Schedule Types based on the requested generated
        public const string ReportScheduleTypesProcedure = "SL_GetReportScheduleTypes";

        //Stored Procedure to create Report Schedules based on the requested generated
        public const string ReportSchedulesCreateProcedure = "SL_CreateReportSchedules";

        //Stored Procedure to Get Report Schedules based on the requested generated
        public const string ReportSchedulesGetProcedure = "SL_GetReportSchedules";

        //Stored Procedure to Delete Report Schedule based on the requested generated
        public const string ReportSchedulesDeleteProcedure = "SL_DeleteReportSchedule";

        //Stored Procedure to Update Report Schedules based on the requested generated
        public const string ReportSchedulesUpdateProcedure = "SL_UpdateReportSchedules";

        //Stored Procedure to Fetch Report Schedules and Process for sending emails
        public const string ReportSchedulesProcessingProcedure = "SL_GetReportScheduleForProcessing";

        //Stored Procedure to Update Report Schedules Processing for sending emails
        public const string ReportUpdateSchedulesProcessingProcedure = "SL_UpdateReportScheduleForProcessing";

        public const string ReportSchedulesDetailsByCodeProcessingProcedure = "SL_GetReportScheduleDetailsByCode";

        public const string ReportScheduleUrlByIdProcedure = "SL_AddReportScheduleForNow";
        #endregion

        #region Reports

        //Stored Procedure to fetch Header Menu/Submenu with their links and other details
        public const string HeaderDetailsProcedure = "SL_GetHeaderDetailsReadonly";

        //Stored Procedure to fetch Shared Report User List based on Company
        public const string SharedReportUserListProcedure = "SL_GetSharedReportUserListByCompany";

        //Stored Procedure to save Shared Report
        public const string SharedReportProcedure = "SL_SaveSharedReport";

        //Stored Procedure to save Archive Report
        public const string ArchiveReportProcedure = "SL_SaveArchiveReport";

        //Stored Procedure to delete Report
        public const string DeleteReportProcedure = "SL_DeleteReport";

        //Stored Procedure to Save and Update Report
        public const string SL_ReportSaveUpdateProcedure = "SL_ReportSaveUpdate";

        #endregion

        #region Activity
        //Stored Procedure to fetch the activity report 
        public const string ActivityProcedure = "SL_GetActivityReadonly";

        //Stored Procedure to fetch the activity alert report 
        public const string ActivityAlertProcedure = "SL_GetActivityAlertReadonly";

        //Stored Procedure to fetch the activity report 
        public const string ActivitySubFilterProcedure = "SL_GetReportSubFilterActivityReadonly";
        #endregion

        #region Labeling

        //Stored Procedure to get all label categories
        public const string GetLabelCatergoryProcedure = "SL_GetLabelCategories";

        //Stored Procedure to save/update label category
        public const string SaveUpdateLabelCategory = "SL_LabelCategorySaveUpdate";

        //Stored Procedure to Label Printing Template Save Update
        public const string LabelPrintingSaveUpdateProcedure = "SL_LabelPrintingTemplateSaveUpdate";

        //Stored Procedure to get Label Printing Template
        public const string LabelPrintingGetTemplateProcedure = "SL_LabelPrintingGetTemplate";

        //Stored Procedure to get Label Printing Component
        public const string LabelPrintingGetComponentProcedure = "SL_LabelPrintingGetComponent";

        //Stored Procedure to get all Label Printing Templates
        public const string LabelPrintingTemplatesProcedure = "SL_LabelPrintingTemplates";

        //Stored Procedure to get all Labels 
        public const string LabelListProcedure = "SL_GetLabels";

        //Stored Procedure to save/update label 
        public const string LabelSaveUpdate = "SL_LabelSaveUpdate";

        //Stored Procedure to save/update label 
        public const string LabelDetailsProcedure = "SL_GetLabelDetails";

        //Stored Procedure to save/update label batch
        public const string LabelBatchSaveUpdate = "SL_LabelBatchSaveUpdate";

        //Stored Procedure to get all Label Batches 
        public const string LabelBatchListProcedure = "SL_GetLabelBatches";

        //Stored Procedure to save/update Label Mapping Store
        public const string LabelMappingStoreSaveUpdateProcedure = "SL_LabelMappingStoreSaveUpdate";

        //Stored Procedure to Fetch Label Auto Mapping  
        public const string LabelAutoMappingDetailProcedure = "SL_GetLabelAutoMappingDetails";

        //Stored Procedure to Fetch Label Mapping Store Details  
        public const string LabelMappingStoreDetailProcedure = "SL_GetLabelMappingStoreDetails";

        //Stored Procedure to save/update Label Batch Mapping Store
        public const string LabelBatchMappingStoreSaveUpdateProcedure = "SL_Lbl_BatchMappingStoreSaveUpdate";

        //Stored Procedure to Fetch Label Batch Auto Mapping  
        public const string LabelBatchAutoMappingDetailProcedure = "SL_GetLabelBatchAutoMappingDetails";

        //Stored Procedure to Fetch Label Batch Mapping Store Details  
        public const string LabelBatchMappingStoreDetailProcedure = "SL_GetLabelBatchMappingStoreDetails";

        public const string LabelCategoryDeleteProcedure = "SL_DeleteLabelCategory";
        public const string LabelTemplateDeleteProcedure = "SL_DeleteLabelTemplate";
        public const string LabelDeleteProcedure = "SL_DeleteLabel";
        public const string LabelBatchDeleteProcedure = "SL_DeleteLabelBatch";

        #endregion

        #region Fusebill

        //Stored Procedure to Check for Company's Subscription
        public const string CustomerSubscriptionProcedure = "SL_CheckCustomerSubscription";

        //Stored Procedure to Register Customer
        public const string RegisterCustomerProcedure = "SL_RegisterCustomer";

        //Stored Procedure to check Register Customer FusebillId
        public const string RegisterCustomerCheckFusebillIdProcedure = "SL_RegisterCustomerCheckFusebillId";

        //Stored Procedure to check Register Customer CompanyId
        public const string RegisterCustomerCheckCompanyIdProcedure = "SL_RegisterCustomerCheckCompanyId";

        //Stored Procedure to check Register Customer company name
        public const string RegisterCustomerCheckCompanyNameProcedure = "SL_RegisterCustomerCheckCompanyName";

        //Stored Procedure to check Register Customer Login name
        public const string RegisterCustomerCheckLoginNameProcedure = "SL_RegisterCustomerCheckLoginName";

        //Stored Procedure to check Register Customer Payment Address
        public const string RegisterCompanyPaymentAddressProcedure = "SL_RegisterCompanyPaymentAddress";

        //Stored Procedure to Register Customer Status Check Procedure
        public const string SL_RegisterCustomerStatusCheckProcedure = "SL_RegisterCustomerStatusCheck";

        //Stored Procedure to Register Customer Status Check Procedure
        public const string SL_RegisterCustomerActivateProcedure = "SL_RegisterCustomerActivate";

        //Stored Procedure to Register Customer Status Check Procedure
        public const string SL_RegisterCustomerUpdateProcedure = "SL_RegisterCustomerUpdate";

        //Stored Procedure to Register Customer Delete Procedure
        public const string RegisterCustomerDeleteProcedure = "SL_RegisterCustomerDelete";

        //Stored Procedure to Register Customer Update Json Procedure
        public const string RegisterCustomerUpdateJsonProcedure = "SL_RegisterCustomerUpdateJson";

        //Stored Procedure to Register Customer Subscriptions Procedure
        public const string RegisterCustomerSubscriptionsProcedure = "SL_RegisterCustomerSubscriptions";

        //Stored Procedure to get subscriptions
        public const string RegisterCustomerGetFuseBillKeyValueProcedure = "SL_GetFuseBillKeyValue";

        //Stored Procedure to get subscriptions
        public const string RegisterCustomerSaveUpdatePaymentProcedure = "SL_SaveUpdateFuseBillPayment";

        //Stored Procedure to get subscriptions
        public const string RegisterCustomerSaveUpdateSubscriptionProcedure = "SL_SaveUpdateFuseBillSubscription";

        //Stored Procedure to get subscriptions
        public const string FusebillSaveUpdateSubscriptionDetailProcedure = "SL_SaveUpdateFuseBillSubscriptionDetail"; 

        //Stored Procedure to get subscriptions
        public const string FusebillSaveUpdateCouponDetailProcedure = "SL_SaveUpdateFuseBillCouponDetail";

        //Stored Procedure to get subscriptions
        public const string FusebillDeleteFuseBillCouponDetailProcedure = "SL_DeleteFuseBillCouponDetail";

        //Stored Procedure to get Franchise
        public const string RegisterCustomerGetCustomerFranchisesProcedure = "SL_GetCustomerFranchises";

        //Stored Procedure to get active subscriptions
        public const string SL_GetSubscriptionsProcedure = "SL_GetSubscriptions";

        //Stored Procedure to get fusebill Product subscriptions
        public const string SL_GetFusebillProductSubscriptionsProcedure = "SL_GetFusebillProductSubscriptions";

        #endregion

        #region Location 
        //Stored Procedure to call it from Location Controller to Add Location
        public const string AddLocationProcedure = "SL_BulkStoresCreate";

        //Stored Procedure to call it from Location Controller to Validate Subscription
        public const string ValidateSubscriptionProcedure = "SL_ValidateSubscription";

        #endregion

        #region PrepTimer
        public const string SL_PT_AddCondimentCategory = "SL_PT_AddCondimentCategory";

        public const string SL_PT_AddCondimentCategoryMapping = "SL_PT_AddCondimentCategoryMapping";

        public const string SL_PT_AddCondimentLanguages = "SL_PT_AddLanguages";

        public const string SL_PT_GetCondimentCategories = "SL_PT_GetCondimentCategories";

        public const string SL_PT_GetCondimentCategoryMapping = "SL_PT_GetCondimentCategoryMapping";

        public const string SL_PT_GetLanguages = "SL_PT_GetLanguages";

        public const string SL_PT_GetCondimentCategoryById = "SL_PT_GetCondimentCategoryById";

        public const string SL_PT_GetCondimentCategoryMappingById = "SL_PT_GetCondimentCategoryMappingById";

        public const string SL_PT_GetLanguageById = "SL_PT_GetLanguageById";

        public const string PT_GetLanguages = "PT_GetLanguages";
        public const string PT_AddCondimentLanguages = "PT_AddLanguages";
        public const string PT_GetLanguageById = "PT_GetLanguageById";

        #endregion

        #region Register Equipment Types
        //Stored Procedure SL_EquipmentRegisterGetEquipmentTypes
        public const string SL_EquipmentRegisterGetEquipmentTypes = "SL_EquipmentRegisterGetEquipmentTypes";

        //Stored Procedure SL_EquipmentRegisterFactoryReset
        public const string SL_EquipmentRegisterFactoryReset = "SL_EquipmentRegisterFactoryReset";

        //Stored Procedure SL_EquipmentRegisterValidateEquipment
        public const string SL_EquipmentRegisterValidateEquipment = "SL_EquipmentRegisterValidateEquipment";

        //Stored Procedure SL_Equip_DeviceConfigSave
        public const string SL_Equip_DeviceConfigSave = "SL_Equip_DeviceConfigSave";
        #endregion
    }
}
