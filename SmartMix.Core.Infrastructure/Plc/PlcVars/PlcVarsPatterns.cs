namespace SmartMix.Core.Infrastructure.Plc.PlcVars
{
    public static class PlcVarsPatterns
    {
        /// <summary>
        /// Сетевые без адреса
        /// </summary>
        public static class Free
        {
            /// <summary>
            /// Состояние перевеса/недовеса на бункере
            /// </summary>
            public const string BunkerWeightState = "confBunker{0}WeightState";
        }

        public static class Alm
        {
            public const string Doser = "almBATCHER{0}";
            public const string Accumulator = "almACCUM{0}";
            public const string Mixer = "almMIXER{0}";
            public const string ValveNotOpen = "almValveNotActive";
            public const string ValveNotClose = "almValveNotDeactive";
            public const string SwitchsNotCorrectState = "almSWITCH_NOT_CORRECT";
            public const string ComponentMoreMaxValue = "almMixer{0}ComponentMoreMaxValue";
            public const string ComponentLessMinValue = "almMixer{0}ComponentLessMinValue";
            public const string SwitchImNotCorrectState = "almSWITCH_IM_NOT_CORRECT_STATE";
            public const string SwitchWaterNotCorrectState = "almSWITCH_WATER_NOT_CORRECT_STATE";
            public const string SwitchCementNotCorrectState = "almSWITCH_CEM_NOT_CORRECT_STATE";
            public const string SwitchConcreteNotCorrectState = "almSWITCH_CONCRETE_NOT_CORRECT_STATE";
            public const string LowPressure = "almLOW_PRESSURE";
            public const string BunkerTimeLoad = "almTIME_LOAD_BUNKER";
            /// <summary>0 - скип в аварийном состоянии. 1, 2, 3... - не работает датчик положения #{0}</summary>
            public const string Skip = "almSKIP{0}";
            public const string WaterCounterNoWater = "almWaterCounter{0}NoWater";
            public const string WaterCounterLoseWater = "almWaterCounter{0}LoseWater";
            /// <summary>Время доливки воды закончилось</summary>
            public const string WaterCounterTimeIsOver = "nvoDWD{0}_TimeIsOver";
            public const string TransportLine = "almLINE{0}";
        }

        public static class Conf
        {
            #region Конфигурация

            public const string _1CIntegration = "1CIntegrationActive";

            public const string PanelIntegration = "PanelIntegration";

            public const string TelegramIntegration = "TelegramIntegration";

            public const string TempUserTimeLimit = "TempUserTimeLimit";

            #endregion

            /// <summary>
            /// Блокировщик обновления регистров
            /// </summary>
            public const string LimitDateToUpdateRegs = "LimitDateToUpdateRegs";

            /// <summary>Дискретность отображения влажности на датчике влажности</summary>
            public const string HumiditySensorDiscrete = "HUMIDITY_SENSOR{0}_DISCRETE";
            /// <summary>Дискретность отображения веса на дозаторе</summary>
            public const string DoserDiscrete = "BATCHER{0}_DISCRETE";
            /// <summary>Дискретность отображения веса на накопителе</summary>
            public const string AccumDiscrete = "ACCUM{0}_DISCRETE";
            /// <summary></summary>
            public const string MixerMotoHour = "MIXER{0}_EN_H";

            /// <summary>Объем смесителя (Ёмкость)</summary>
            public const string MixerVolume = "MIXER{0}_VOLUME";

            /// <summary>Время перемешивания общее для всех видов бетонной смеси</summary>
            public const string MixerManualMixTime = "MIXER{0}_MIX_TIME";
            /// <summary>Использовать время перемешивания общее для всех видов бетонной смеси (MixTimeMode)</summary>
            public const string MixerUseManualMixTime = "MIXER{0}_USE_COMMON_MIX_TIME";
            /// <summary>Использовать ли ручное упреждение</summary>
            public const string BunkerUseManualFore = "BUNKER{0}_USE_MANUAL_ANTICIPATION";
            /// <summary>Ручное упреждение</summary>
            public const string BunkerManualFore = "BUNKER{0}_FORE";
            /// <summary>Использовать/не использовать пересчет по влажности</summary>
            public const string BunkerUseHumidity = "BUNKER{0}_HUMIDITY";
            /// <summary></summary>
            public const string BunkerTitle = "BUNKER{0}_TITLE";
            /// <summary>"Использовать датчик влажности" на бункере. ( BUNKER{0}_USING_OF_HUMIDITY_SENSOR )</summary>
            public const string BunkerUsingOfHumiditySensor = "BUNKER{0}_USING_OF_HUMIDITY_SENSOR";
            public const string WaterCounterTotalAmount = "WaterCounter{0}TotalAmount";

            //
            public const string LockMixerAfterBatch = "MIXER{0}_LOCK_AFTER_BATCH";
            public const string LockMixerAfterApp = "MIXER{0}_LOCK_AFTER_APP";

            public const string MixerCurLog = "MIXER{0}_CUR_LOG_ENABLED";

            public const string DoserInMixerEnabled = "MIXER{0}_DOSER{1}_ON";

            public const string DoserPercentageOverweight = "nciBATCHER{0}_AllowedOverweightPercent";
            public const string DoserPercentageOverweightBlock = "nciBATCHER{0}_AllowedOverweightPercent_Block";
            public const string DoserPercentageUnderweight = "nciBATCHER{0}_AllowedUnderweightPercent";
            public const string DoserPercentageUnderweightBlock = "nciBATCHER{0}_AllowedUnderweightPercent_Block";

            /// <summary>Промывка только в последнем замесе</summary>
            public const string IsLastNeedWash = "confBATCHER{0}_WashOnlyLast";

            /// <summary> Процент потока бункера </summary>
            public const string BunkerFlowPercent = "BUNKER{0}_ADVANCING_FLOW_PERCENT";

            /// <summary>Максимальное значение датчика </summary>
            public const string MaxValueConsumption = "BUNKER{0}_MAX_VALUE";

            #region Задачи системы

            /// <summary>
            /// Включить задание "Автозавершение рабочей смены".
            /// </summary>
            public const string IsActiveJobChange = "IsActiveJobChange";

            /// <summary>
            /// Время завершения рабочей смены
            /// </summary>
            public const string TimeJobChange = "TimeJobChange";

            /// <summary>
            /// Сбросить все корректировки в рецептах по завершению рабочей смены.
            /// </summary>
            public const string IsResetJobChange = "IsResetJobChange";

            /// <summary>
            /// Время выполнения задания по автоматической генерации отчетов.
            /// </summary>
            public const string AutoSaveReport = "AutoSaveReport";

            #endregion Задачи системы

            /// <summary>
            /// Начало названия файла в автоматической системе сохранения отчётов
            /// </summary>
            public const string NameReportFile = "NameReportFile";

            public const string RecalculationSetting = "RecalculationSetting";

            public const string KeepLogDayCyclogram = "KeepLogDayCyclogram";

            public const string KeepLogDay = "KeepLogDay";

            #region Настройки шага

            public const string AccuracyOfIMSetting = "AccuracyOfIMSetting";

            public const string AccuracyOfCemSetting = "AccuracyOfCemSetting";

            public const string AccuracyOfWaterSetting = "AccuracyOfWaterSetting";

            public const string AccuracyOfHdSetting = "AccuracyOfHdSetting";

            #endregion

            public const string MaxOfIMSetting = "MaxOfIMSetting";

            public const string MaxOfCemSetting = "MaxOfCemSetting";

            public const string MaxOfWaterSetting = "MaxOfWaterSetting";

            public const string MaxOfHdSetting = "MaxOfHdSetting";

            public const string MinOfIMSetting = "MinOfIMSetting";

            public const string MinOfCemSetting = "MinOfCemSetting";

            public const string MinOfWaterSetting = "MinOfWaterSetting";

            public const string MinOfHdSetting = "MinOfHdSetting";

            /// <summary>Время движения скипа от среднего до верхнего</summary>
            public const string SkipTopMoveDuration = "SKIP{0}_TOP_MOVE_TIME";
            /// <summary>Время движения скипа от нижнего до среднего</summary>
            public const string SkipBottomMoveDuration = "SKIP{0}_BOTTOM_MOVE_TIME";

            #region Консистенция

            /// <summary>Разрешение на выгрузку настройка в смесителе</summary>
            public const string ConsistencyPermissionUpload = "ConsistencyPermissionUpload{0}";
            /// <summary>Разрешение пользователя на выгрузку по кнопке </summary>
            public const string ConsistencyUserButton = "IsButtonUserCons_{0}";
            /// <summary>Пластичность в не норме </summary>
            public const string BadConsistency = "IsBadConsistency_{0}";

            #endregion
        }

        /// <summary>Настройки</summary>
        public static class Nci
        {
            public const string BatcherMinWeightChange = "nciBatcher{0}MinWeightChange";
            public const string BatcherMinWeightChangeAlmTime = "nciBatcher{0}MinWeightChangeAlmTime";
            public const string TransferWeight = "nciTransferWeight{0}";
            public const string TransferTime = "nciTransferTime{0}";
            public const string TimeFilling = "nciTime_Filling";
            public const string TimePumping = "nciTime_Pumping";
            public const string Batch2TransferTime = "nciBatcher2TransferTime{0}";
            public const string Batcher2TransferWeight = "nciBatcher2TransferWeight{0}";
            /// <summary>
            /// Этап выполнения замеса: 
            /// 0 - Свободный, 
            /// 1 - чтение исходных данных, 
            /// 2 - дозирование, 
            /// 3 - ожидание команды на выгрузку, 
            /// 4 - ожидание времени до начала выгрузки, 
            /// 5 - выгрузка, 
            /// 6 - замес завершен
            /// </summary>
            public const string MixerBatchPhase = "nciMIXER{0}_BATCH_PHASE";

            /// <summary>Этап выполнения замеса: 
            /// 0 - Свободный, 
            /// 1 - чтение исходных данных, 
            /// 2 - дозирование, 
            /// 3 - ожидание команды на выгрузку, 
            /// 4 - ожидание времени до начала выгрузки, 
            /// 5 - выгрузка, 6 - замес завершен. ( nciBATCHER{0}_BATCH_PHASE )</summary>
            public const string DoserBatchPhase = "nciBATCHER{0}_BATCH_PHASE";
            /// <summary>Статус</summary>
            public const string SkipBatchPhase = "nciSKIP{0}_BATCH_PHASE";
            public const string AccumulatorBatchPhase = "nciACCUM{0}_BATCH_PHASE";
            public const string BunkersUse = "nciBUNKER_USE";
            public const string BunkerMinFore = "nciBUNKER{0}_MIN_FORE";
            public const string Fore = "nciPR{0}_FORE";
            public const string DoserStikyMass = "nciBATCHER{0}_W_NALIP";
            public const string DoserMaxWeight = "nciBATCHER{0}_W_MAX";
            public const string DoserMaxDosingTime = "nciBATCHER{0}_DOSING_TIME_MAX";
            public const string DoserMinUnloadTime = "nciBATCHER{0}_UNLOAD_TIME_MIN";
            public const string DoserMaxUnloadTime = "nciBATCHER{0}_UNLOAD_TIME_MAX";
            public const string DoserFilterValue = "nciAI_{0}_f";
            public const string DoserCorrectPercent = "nciBATCHER{0}_PERCENT_CORRECT";
            public const string DoserRelaxTime = "nciBATCHER{0}_TIME_RELAX";
            public const string DoserLoadTime = "nciBATCHER{0}_TIME_LOAD";
            public const string DoserCoefK = "nciAI_{0}_k";
            public const string DoserCoefB = "nciAI_{0}_b";
            public const string DoserAccuracy = "nciBATCHER{0}_DOSE_ACCURACY";
            public const string BunkerAutoLoad = "nciBUNKER_MODE_LOAD";
            public const string TrailerOff = "nciVALVE_NOT_CORRECT_REACTION";
            public const string ValveImpulseTime = "nciVALVE{0}_TIME_MAN_ACTIVE";
            /// <summary>
            /// Время работы (в секундах)
            /// </summary>
            public const string ValveTimeToOpenAlarm = "nciVALVE{0}_TIME_TO_ACTIVE";
            /// <summary>
            /// Время ожидания (в секундах)
            /// </summary>
            public const string ValveTimeToCloseAlarm = "nciVALVE{0}_TIME_TO_DEACTIVE";
            public const string ValveUnloadMode = "nciVALVE_ACTIVE_MODE";
            public const string DoserUnloadSpeed = "nciUnloadSpeedSettings{0}";
            /// <summary>Скорость высыпания для включения вибрации </summary>
            public const string ViberOutFlowSpeed = "nciVIBR{0}_WEIGHT_START";
            /// <summary>Время работы за одно дозирование </summary>
            public const string ViberWorkTime = "nciVIBR{0}_TIME_WORKING";
            /// <summary>Время одного импульса </summary>
            public const string ViberPulseTime = "nciVIBR{0}_TIME_HIGH_PULSE";
            /// <summary>Время паузы между импульсами </summary>
            public const string ViberDelayTime = "nciVIBR{0}_TIME_LOW_PULSE";
            /// <summary>Автоматический режим вибрации. Массив</summary>
            public const string ViberUse = "nciUSE_VIBR";
            /// <summary>Автоматическое обнуление дозатора перед замесом Массив</summary>
            public const string BatcherAutoSet0 = "nciBATCHER{0}AutoSet0";
            public const string MixerUnloadTime = "nciMIXER{0}_UNLOAD_TIME";
            public const string MixerUnloadExtraTime = "nciMIXER{0}_UNLOAD_EXTRA_TIME";
            /// <summary>Режим ручной выгрузки смесителя: 0 - как в автоматическом режиме, 1 - импульс, 2 - по команде</summary>
            public const string MixerUnloadMode = "nciMIXER{0}_UNLOAD_MODE";
            public const string BatcherManualUnloadMode = "nciBATCHER_MANUAL_UNLOAD_MODE";
            /// <summary>
            /// Номер затвора в смесителе, из которого нужно выгружать ( nciMIXER{0}_GATE_NUMBER )
            /// </summary>
            public const string MixerGateNumber = "nciMIXER{0}_GATE_NUMBER";
            //public const string MixerImpulseTime = "nciMIXER{0}_UNLOAD_PULSE_TIME_ACTIVE";
            /// <summary>Количество импульсов при выгрузке из смесителя 
            /// nciMixer{номер смесителя}_Gate{порядковый номер затвора}_PULSE_COUNT_UNLOAD</summary>
            public const string ImpulseCountUnloadValve = "nciMixer{0}_Gate{1}_PULSE_COUNT_UNLOAD";
            public const string MixerTimeToStart = "nciTIME_TO_START_{0}";
            public const string MixerTimeToStop = "nciTIME_TO_STOP_{0}";

            /// <summary>
            /// Представляет признак ручного режима ПО.
            /// </summary>
            public const string ManualMode = "nciMANUAL";

            /// <summary>
            /// Представляет признак полуавтоматического режима ПО.
            /// </summary>
            public const string HalfAutoMode = "nciHALF_AUTO";

            /// <summary>
            /// Признак тренажёрного режима.
            /// </summary>
            public const string TrainMode = "nciTRAINER";
            public const string BlockMode = "nciALARM";
            public const string MixerUnloadLock = "nciMIXER{0}_UNLOAD_LOCK";
            public const string PressureUsing = "nciPRESSURE_USE";
            public const string ValveTimeWorking = "nciVALVE{0}_TIME_WORKING";
            public const string ControlPanelUsing = "nciUSE_CONTROL_PANEL_{0}";
            public const string MixerGateManualUnloadMode = "nciMixer{0}_Gate{1}_UNLOAD_MANUAL_MODE";
            /// <summary> Режим работы автосмазчика смесителя. Номер по номеру смесителя</summary>
            public const string LubricatorWorkMode = "nciLubricator{0}_MODE";
            /// <summary> Время работы автосмазчика </summary>
            public const string LubricatorTimeWorking = "nciLubricator{0}_TimeWorking";
            /// <summary> Время ожидания между циклами работы </summary>
            public const string LubricatorTimeDelay = "nciLubricator{0}_TimeRest";
            public const string PressureMinValue = "nciPRESSURE_MIN_VALUE";
            public const string SwitchTime = "nciSWITCH_TIME";
            public const string BunkerTimeLoad = "nciBUNKER{0}_TIME_LOAD";
            public const string MixerTimeDelayToActive = "nciMIXER{0}_GATE1_SENSOR1_TIME_DELAY_TO_ACTIVE";
            /// <summary>
            /// nciMIXER{0}_GATE{1}_SENSOR{2}_TIME_DELAY_TO_ACTIVE
            /// </summary>
            public const string MixerTimeDelayToActive3Params = "nciMIXER{0}_GATE{1}_SENSOR{2}_TIME_DELAY_TO_ACTIVE";
            public const string BunkerMindose = "nciBUNKER{0}_MINDOSE";
            public const string BunkerValveActivePulseMin = "nciBUNKER{0}_VALVE_ACTIVE_PULSE_MIN";
            /// <summary>Время движения до датчика положения #{0}</summary>
            public const string SkipTimeMotionToSensor = "nciSKIP{0}_TIME_MOTION_TO_SENSOR{1}";

            /// <summary>Минимальное время выгрузки</summary>
            public const string AccumUnloadTimeMin = "nciACCUM_UNLOAD_TIME_MIN";
            /// <summary>Минимальное время выгрузки</summary>
            public const string SkipUnloadTimeMin = "nciSKIP{0}_UNLOAD_TIME_MIN";
            /// <summary>Минимальное время выгрузки</summary>
            public const string SkipUnloadLock = "nciSKIP{0}_UNLOAD_LOCK";

            /// <summary>СВИ {0} - литров  за импульс</summary>
            public const string WaterCounterImpulseAmount = "nciWaterCounter{0}ImpulseAmount";
            /// <summary>СВИ {0} - Время до аварии "Нет воды"</summary>
            public const string WaterCounterTimeNoWaterAlarm = "nciWaterCounter{0}TimeNoWaterAlarm";
            /// <summary>СВИ {0} - Время до аварии "Утечка воды", с</summary>
            public const string WaterCounterTimeLoseWaterAlarm = "nciWaterCounter{0}TimeLoseWaterAlarm";

            /// <summary>ПДВ {0} - время ожидания между тактами доливки, мс</summary>
            public const string DWDCooldownTime = "nciDWD{0}_CooldownTime";

            public const string TimeBeforeStart = "nciDWD{0}_TimeBeforeStart";

            /// <summary>ПДВ {0} - гистерезис влажности при доливке</summary>
            public const string DWDMoistureHysteresis = "nciDWD{0}_MoistureHysteresis";
            /// <summary>Фиксация нормальной влажности </summary>
            public const string DWDMoistureNotOkDelay = "nciDWD{0}MoistureNotOkDelay";
            /// <summary>Задержка анализа влажности</summary>
            public const string MixerMoistureOkDelay = "nciMixer{0}MoistureOkDelay";

            /// <summary>Минимальный объём воды для промывки дозатора {0}</summary>
            public const string MinWeightWash = "nciMinWeightWash{0}";

            /// <summary>Максимальное время промывки дозатора {0}</summary>
            public const string MaxTimeWash = "nciMaxTimeWash{0}";

            /// <summary>Вес дозатора {0}, при котором необходимо выключить ИМ, кг</summary>
            public const string WeightDeactiveValve = "nciWeightDeactiveValve{0}";

            /// <summary>Режим доливки ( nciDWD{0}_Mode )</summary>
            public const string DwdMode = "nciDWD{0}_Mode";

            /// <summary>Объём доливаемой воды за цикл, л</summary>
            public const string DwdAutoWaterAmount = "nciDWD{0}_DoseAmountAuto";

            /// <summary>Макс время доливки</summary>
            public const string DwdAutoMaxDosingTime = "nciDWD{0}_MaxDosingTime";

            /// <summary>Использовать сброс лишнего материала</summary>
            public const string UseDropingExcessMaterial = "nciUseReversDosing";

            /// <summary>Использовать двухэтапное дозирование</summary>
            public const string UseTwoStepDosing = "nciTwoStepDosingUse";

            /// <summary>( "nciSENSOR_OFF" )</summary>
            public const string SensorOff = "nciSENSOR_OFF";

            /// <summary>( "nciMixer{0}_Check_Moisture" )</summary>
            public const string CheckMoisture = "nciMixer{0}_Check_Moisture";

            /// <summary>Проверять влажность в смесителе - "nciMixer{0}_Normal_Moisture_Interval"</summary>
            public const string MixerNormalMoistureInterval = "nciMixer{0}_Normal_Moisture_Interval";

            //             /// <summary>Включить динамический пересчёт уставки в режиме АВТО - ( "nciDWD{0}_Use_Dynamic_SP_Recalc" )</summary>
            //public const string DwdUseDynamicSpRecalc = "nciDWD{0}_Use_Dynamic_SP_Recalc";

            /// <summary>Объем воды необходимый для увеличения влажности на 1 пункт ( "nciDWD{0}_Correction_Coef" )</summary>
            public const string CorrectionCoef = "nciDWD{0}_Correction_Coef";

            //          /// <summary>(Процент доливки воды "nciDWD{0}_Refilling_Percentage" )</summary>
            //        public const string RefillingPercentage = "nciDWD{0}_Refilling_Percentage";

            public const string AccumulatorRelaxTime = "nciACCUM{0}_TIME_RELAX";
            public const string AccumulatorUnloadLock = "nciACCUM{0}_UNLOAD_LOCK";
            public const string AccumulatorUnloadMinTime = "nciACCUM{0}_UNLOAD_TIME_MIN";
            public const string TransportLineAutoStopTime = "nciLINE{0}_TIME_AUTOSTOP";
            public const string TransportLineAutoStopUse = "nciLINE{0}_AUTOSTOP_USE";

            public const string UseBunkerInAccuracyMode = "nciUseBunkerInPreciseDosing";

            /// <summary> Максимальная погрешность дозирования материала </summary>
            public const string LineWarmUp = "nciLINE{0}_WARM_UP_TIME";

            /// <summary> Максимальная погрешность дозирования материала </summary>
            public const string MixerMaterialCheckPercent = "nciMaterialCheckPercent";


            /// <summary> Не закрывать затвор смесителя после импульса. 
            /// nciMixer{Номер миксера}_Gate{Порядковый номер затвора}_Dont_Close_After_Impulse </summary>
            public const string GateDontCloseAfterImpulse = "nciMixer{0}_Gate{1}_Dont_Close_After_Impulse";

            /// <summary> Использовать настройки выгрузки из рецепта
            public const string UseGateSettingsFromRecipe = "nciMixer{0}_Gate{1}_UseSettingsFromRecipe";

            /// <summary>Максимальный ток смесителя</summary>
            public const string MixerMaxCurrent = "nciMixer{0}_Max_Current";

            /// <summary>Максимальный ток, при котором возможна загрузка в смеситель в автомате</summary>
            public const string MixerMaxCurrentLoad = "nciMixer{0}_Max_Current_Load";

            /// <summary>Задержка разрешения выгрузки в смеситель по току</summary>
            public const string MixerMaxCurrentLoadDelay = "nciMixer{0}_Current_Load_Delay";

            /// <summary>Разрешить алгоритм проверки тока в смесителе</summary>
            public const string MixerEnableCurrentCheck = "nciMixer{0}_EnableCurrentCheck";

            /// <summary>Начинать следующий замес при простое любого из дозаторов</summary>
            public const string EnableBatchCrossing = "nciEnableBatchCrossing";

            /// <summary>Разрешить алгоритм проверки крышки люка смесителя </summary>
            public const string MixerEnableHatchCheck = "nciMixer{0}_EnableHatchCheck";

            /// <summary>Запрос перезагрузки настроек</summary>
            public const string QueryReloadSettings = "nciRequestSettings";

            /// <summary>Ток для настройки калибровки датчика уровня</summary>
            public const string CalibTableX = "nciCalibTableX{0}";

            /// <summary>Уровень для настройки калибровки датчика уровня</summary>
            public const string CalibTableY = "nciCalibTableY{0}";

            public const string SystemTime = "nciTIME_FOR_LICENSE";

            /// <summary>
            /// Налипший вес
            /// </summary>
            public const string AccumWeightNalip = "nciAccum{0}_WeightNalip";
            /// <summary>Выгрузка по весу</summary>
            public const string AccumUseWeightUnload = "nciAccum{0}_UseWeightUnload";

            /// <summary>
            /// Максимальное время выгрузки
            /// </summary>
            public const string AccumMaxTimeUnload = "nciAccum{0}_MaxTimeUnload";
            public const string FC_Speed = "nciSpeed_FC_{0}";

            public const string AdvancingFlowPercent = "nciPR{0}_ADVANCING_FLOW_PERCENT";

            public const string UseBottomSensor = "nciSKIP{0}_USE_BOTTOM_SENSOR";

            /// <summary>Начать выгрузку за ____ с</summary>
            public const string SkipStartBeforeBatchEndTime = "nciSkip{0}StartBeforeBatchEndTime";

            /// <summary> Процент потока бункера </summary>
            public const string FlowPercent = "nciPR{0}_ADVANCING_FLOW_PERCENT";

            public const string AutoOffDelay = "nciMIXER{0}_AUTO_OFF_DELAY_TIME";

            public const string AutoOffUsing = "nciMIXER{0}_AUTO_OFF_USING";

            /// <summary>
            /// Максимальная грузоподъемность
            /// </summary>
            public const string MaxWeightSkip = "nciSKIP{0}_MaxWeight";

            /// <summary>
            /// Максимальная грузоподъемность
            /// </summary>
            public const string MaxWeightAccum = "nciAccum{0}_MaxWeight";

            /// <summary>
            /// Минимальный процент дозирования материала при быстрой заявке
            /// </summary>
            public const string BatcherFastDosingPersentage = "nciBATCHER{0}FastDosingPersentage";

            #region Влажность по рецепту

            /// <summary>
            /// Использовать доливку по требуемому ВЦ
            /// 0 - Номер смесителя
            /// </summary>
            public const string UseWCALGMixer = "nciUse_WC_ALG_M{0}";

            /// <summary> Использовать в расчетах максимальное значение влажности </summary>
            public const string UseMaxMoisture = "nciM{0}UseMaxMoisture";

            /// <summary>
            /// Минимальный объем доливаемой воды
            /// </summary>
            public const string FlowmeterMinDose = "nciMixer{0}FlowmeterMinDose";

            public const string WaterDosingOneStep = "nciFlowmeter{0}DosingOneStep";

            public const string WaterDosingAccuracy = "nciFlowmeter{0}Accuracy";

            /// <summary>
            /// Упреждение затвора расходомера, кг
            /// </summary>
            public const string FlowmeterFore = "nciFlowmeter{0}Fore";

            /// <summary>
            /// Время импульса затвора расходомера, мс
            /// </summary>
            public const string FlowmeterTimePulse = "nciFlowmeter{0}TimePulse";

            /// <summary>
            /// Время между импульсами затвора расходомера, мс
            /// </summary>
            public const string FlowmeterTimeRelax = "nciFlowmeter{0}TimeRelax";

            #endregion Влажность по рецепту

            /// <summary>
            /// Минимальный расход расходомера
            /// </summary>
            public const string WaterCounterSetting = "nciWaterCounter{0}MinFLow";

            /// <summary>
            /// Пауза на смесителе
            /// </summary>
            public const string MIXER_PAUSE = "nciMIXER{0}_PAUSE";

            /// <summary>
            /// Требуемый номер силоса
            /// </summary>
            public const string TragetSilosNum = "nciTragetSilosNum";

            #region Кюбель

            public const string TimeToOpenValve = "nciKubel{0}TimeToOpenValve";

            public const string TimeToCloseValve = "nciKubel{0}TimeToCloseValve";

            public const string KubelUnloadingTime = "nciKubel{0}UnloadingTime";

            public const string KubelAutoUnload = "nciKubel{0}AutoUnload";

            public const string KubelAutoReturn = "nciKubel{0}AutoReturn";


            public const string KubelMaxMovigTimePassBy = "nciKubel{0}TimeMovingBetweenPost{1}PassBy";
            public const string KubelMaxMovigTimeSlowDown = "nciKubel{0}TimeMovingBetweenPost{1}SlowDown";
            public const string KubelVibrPulseTime = "nciKubel{0}VIBR_TIME_HIGH_PULSE";
            public const string KubelVibrRelaxTime = "nciKubel{0}VIBR_TIME_LOW_PULSE";
            public const string KubelUseVibr = "nciKubel{0}UseVibr";
            #endregion
        }

        /// <summary>
        /// Newtwork Variable Iutput - только запись
        /// </summary>
        public static class Nvi
        {
            public const string BATCH_ADD_ANS = "nviBATCH_ADD_ANS";
            public const string ANS_FORE = "nviANS_FORE";
            public const string BATCH_SAVE_REQ = "nviBATCH_SAVE_REQ";
            public const string ResetControlPanel = "nviRESET_CONTROL_PANEL_{0}";
            public const string DoserTimeStartUnload = "nviBATCHER{0}_TIME_START_UNLOAD";
            public const string DoserCalibWeight = "nviAI_{0}_CalibValue";

            public const string BunkerPriority = "nviBUNKER{0}_PR";

            public const string PrioritetWeight = "nviPR{0}_WEIGHT";
            public const string ApplicationNumber = "nviREQ_NUMBER";
            public const string BatchNumber = "nviBATCH_NUMBER";
            public const string ManualCorectWeight = "nviBUNKER{0}_CORRECT_WEIGHT";
            /// <summary>nviVALVE_ACTIVE_OVD</summary>
            public const string ValveCommandSet = "nviVALVE_ACTIVE_OVD";
            /// <summary>nviVALVE_DEACTIVE_OVD</summary>
            public const string ValveCommand = "nviVALVE_DEACTIVE_OVD";
            public const string MixerMotoHour = "nviMIXER{0}_EN_H";
            public const string UseAutoCorrect = "nviUSE_AUTO_CORRECT";
            /// <summary>nviVIBR_OVD</summary>
            public const string VibrationCommand = "nviVIBR_OVD";
            public const string DoserUnloadLock = "nviBATCHER_UNLOAD_LOCK";
            public const string DoserLoadPause = "nciLOAD_PAUSE";
            public const string StartCommand = "nviSTART_CMD";
            public const string StopCommand = "nviSTOP_CMD";
            public const string AlarmReset = "nviALARM_RESET";
            public const string AllowMixerUnload = "nviALLOW_UNLOAD_MIXER{0}";
            //Флаг изменения статуса смесителя
            public const string FlagOfStatusChangingOfMixer = "nviFLAG_EDIT_BATCH_PHASE_MIXER{0}";
            public const string DoserCalibrationSet0 = "nviAI_Calib_Set0";
            public const string SkipCalibrationSet0 = "nviAccum{0}SetNull";
            public const string DoserCalibrationCommand = "nviAI_Calib_Cmd";
            public const string BunkerHumidityCorrectWeight = "nviBUNKER{0}_HUMIDITY_CORRECT_WEIGHT";
            public const string MixerNumber = "nviMIXER_NUMBER";
            public const string MixerMixTime = "nviMIXER_MIX_TIME";
            /// <summary>СВИ {0} - общий счётчик</summary>
            public const string WaterCounterTotalAmount = "nviWaterCounter{0}TotalAmount";

            /// <summary>Продолжить промывку: 0й бит -1й дозатор и т.д.</summary>
            public const string WashExtend = "nviWashExtend";

            /// <summary>Завершить промывку: 0й бит -1й дозатор и т.д.</summary>
            public const string WashEnd = "nviWashEnd";

            /// <summary>
            /// nviDWD{0}_MoistureSP
            /// </summary>
            public const string MixerHudimity = "nviDWD{0}_MoistureSP";

            /// <summary>ПДВ 1 - объём доливаемой воды  при режиме ПОЛУ-АВТО ( nviDWD{0}_DoseAmountHalfAuto )</summary>
            public const string DoseAmountHalfAuto = "nviDWD{0}_DoseAmountHalfAuto";
            /// <summary>
            /// nviBatch_Moisture_SP
            /// </summary>
            public const string BatchHudimity = "nviBatch_Moisture_SP";

            public const string BatchVolume = "nviBATCH_VOLUME";

            public const string ConfirmBadMoisture = "nviConfirm_Bad_Moisture_{0}";
            public const string DwdStartDose = "nviDWD{0}_Start_Dose";
            public const string DwdStopDose = "nviDWD{0}_Stop_Dose";
            /// <summary>Команда калибровки</summary>
            public const string CalibTableCommand = "nviCalibTableCommand{0}";
            /// <summary>
            /// Флаг изменения переменных nci на контроллере. Необходимо обновить регистры на сервере.
            /// </summary>
            public const string NeedReloadNci = "nviFLAG_UPDATE_NCI";
            /// <summary>Указать дозатору промывку в данном замесе</summary>
            public const string DoserNeedWash = "nviNeedWash{0}";
            /// <summary>Подтверждение, что накопитель пустой</summary>
            public const string AccumConfirmEmpty = "nviAccum{0}_Confirm_Empty";
            /// <summary>последний замес</summary>
            public const string LastBatch = "nviLastBatch";

            /// <summary>Быстрая заявка</summary>
            public const string QuickReq = "nviQuickReq";

            #region Моточасы

            /// <summary>Моточасы valve</summary>
            public const string ValveMotoHour = "nviVALVE{0}_EN_H";
            /// <summary>Моточасы vibr</summary>
            public const string VibrMotoHour = "nviVIBR{0}_EN_H";
            /// <summary>Моточасы start</summary>
            public const string StartMotoHour = "nviSTART{0}_EN_H";

            #endregion

            public const string UnloadingPoint = "nviUnloadingPoint";

            #region Настройки на замес смеситель

            /// <summary>Время выгрузки</summary>
            public const string MixerUnloadTime = "nviMIXER_UNLOAD_TIME";
            /// <summary>Время выгрузки</summary>
            public const string TimeExtraUnload = "nviMixerUnloadExtraTime";
            /// <summary>Режим работы</summary>
            public const string MixerUnloadMode = "nviMixer_Unload_Mode";

            /// <summary>Количество импульсов</summary>
            public const string MixerPulseCountUnload = "nviMixer_Gate_PULSE_COUNT_UNLOAD";

            /// <summary>Время импульсов</summary>
            public const string MixerTimePulse = "nviMixer_Gate_Time_Pulse";
            /// <summary>Задержка между импульсами</summary>
            public const string MixerTimeDelayPulse = "nviMixer_Gate_Time_Delay_Pulse";
            /// <summary>Не закрывать после импульса</summary>
            public const string MixerDontCloseAfterImpulse = "nviMixer_Gate_Dont_Close_After_Impulse";
            /// <summary>Задержка на открытие 1 - 50, 2 - 75 </summary>
            public const string MixerSenserTimeDelayToActive = "nviMIXER_GATE_SENSOR{0}_TIME_DELAY_TO_ACTIVE";
            public const string TurnerTargetBunker = "nviTurnerTargetBunker";

            #endregion

            /// <summary>Сброс ПЛК</summary>
            public const string ResetPlc = "nviResetPLC";

            #region Новая доливка
            /// <summary>
            /// необходимость калибровки рецепта по влажности
            /// </summary>
            public const string BatchMoistureCoef = "nviBatch_Moisture_Coef";

            /// <summary>
            /// Требуемая влажность в смесителе
            /// </summary>
            public const string Batch_Moisture_SP = "nviBatch_Moisture_SP";

            /// <summary>
            /// Осуществляется ли калибровка рецепта
            /// </summary>
            public const string CalibCurrentRecipe = "nviCalibCurrentRecipe";

            /// <summary>
            /// Объем рецепта для калибровки (мин, сред, макс)
            /// </summary>
            public const string CalibratedRecipeVolumeSelect = "nviCalibVolumeSelector";

            /// <summary>
            /// нескорректированное по влажности значение воды
            /// </summary>
            public const string BatchWaterUncorrected = "nviBatchWaterUncorrected";

            public const string Batch_Moisture_Steep = "nviBatch_Moisture_Steep";

            public const string Batch_Moisture_Ofset = "nviBatch_Moisture_Ofset";

            #region DryMix
            public const string StatrDrying = "nviStartDrying";
            public const string StopDrying = "nviStopDrying";
            public const string ResetDrying = "nviResetDryingAlg";
            #endregion

            #endregion Новая доливка

            #region Кюбель

            public const string KubelModeAuto = "nviKubel{0}ModeAuto";

            public const string TargetPost = "nviKubel{0}TargetPost";

            public const string KubelGateOpen = "nviKubel{0}Gate1OpenCMD";

            public const string KubelGateClose = "nviKubel{0}Gate1CloseCMD";

            public const string KubelForward = "nviKubel{0}ForwardCMD";

            public const string KubelBackward = "nviKubel{0}BackwardCMD";

            public const string KubelResetMoving = "nviKubel{0}ResetMovingCMD";

            public const string KubelLock = "nviKubel{0}Lock";

            public const string KubelResetAlg = "nviKubel{0}ResetAlgCMD";

            public const string KubelResetAlarms = "nviKubel{0}ResetAlarms";

            public const string KubelVibrCMD = "nviKubel{0}VibrCMD";

            /// <summary>Команда на выгрузку из кюбеля от оператора</summary>
            public const string KubelOperUnloadCMD = "nviKubel{0}OperUnloadCMD";
            #endregion
        }

        /// <summary>
        /// Newtwork Variable Output - только чтение
        /// </summary>
        public static class Nvo
        {
            public const string REQ_FORE = "nvoREQ_FORE";
            public const string BATCH_SAVE_ANS = "nvoBATCH_SAVE_ANS";
            public const string BATCH_ADD_REQ = "nvoBATCH_ADD_REQ";
            public const string BATCH_SAVE_COUNT = "nvoBATCH_SAVE_COUNT";
            public const string PR_FORE = "nvoPR_FORE";
            public const string NUMBER_BATCHER_FORE = "nvoNUMBER_BATCHER_FORE";
            public const string WEIGHT_FORE = "nvoWEIGHT_FORE";

            public const string MixerReadyToUnload = "nvoMixer{0}_Ready_To_Unload";

            /// <summary>
            /// Номер заявки, с которой работает дозатор ( "nvoBATCHER{0}_REQ_POINT" )
            /// </summary>
            public const string DoserApplication = "nvoBATCHER{0}_REQ_POINT";

            /// <summary>
            /// Последний замес ( "nvoBATCHER{0}_LAST_BATCH" )
            /// </summary>
            public const string DoserLastBatch = "nvoBATCHER{0}_LAST_BATCH";

            /// <summary>
            /// Номер замеса, с которым работает дозатор ( "nvoBATCHER{0}_BATCH_POINT" )
            /// </summary>
            public const string DoserBatch = "nvoBATCHER{0}_BATCH_POINT";
            public const string AccumulatorApplication = "nvoACCUM{0}_REQ_POINT";
            public const string AccumulatorBatch = "nvoACCUM{0}_BATCH_POINT";
            public const string AccumulatorLastBatch = "nvoACCUM{0}_LAST_BATCH";

            /// <summary>
            /// Время до начала выгрузки из накопителя
            /// </summary>
            public const string AccumulatorTimeToUnloadStart = "nvoACCUM{0}_REMAIN_TIME_START_UNLOAD";
            public const string AccumulatorUnloadTime = "nvoACCUM{0}_REMAIN_TIME_UNLOAD";
            public const string AccumulatorBunkerWeight = "nvoACCUM{0}_BUNKER{1}_WEIGHT";
            public const string BunkerAutoCounter = "nvoCOUNT_AUTO_{0}";
            public const string BunkerManualCounter = "nvoCOUNT_MANUAL_{0}";
            public const string BunkerAutoCounterSave = "nvoSAVE_COUNT_AUTO_{0}";
            public const string BunkerManualCounterSave = "nvoSAVE_COUNT_MANUAL_{0}";
            public const string BunkerCorrectCounter = "nvoBUNKER{0}_CORRECT_WEIGHT";
            public const string BunkerCorrectCounterSave = "nvoSAVE_BUNKER{0}_CORRECT_WEIGHT";
            public const string BunkerNeedWeight = "nvoBUNKER{0}_NEED_WEIGHT";

            public const string BunkerPriority = "nvoBUNKER{0}_PR";
            public const string PlcReady = "nvoPLC_READY";
            /// <summary>Номер заявки, отправленной на сохранение в SCADA ( "nvoREQ_NUMBER" )</summary>
            public const string ApplicationNumber = "nvoREQ_NUMBER";
            /// <summary>Номер замеса, который выставлен для сохранения в скаду ( "nvoBATCH_NUMBER" )</summary>
            public const string BatchNumber = "nvoBATCH_NUMBER";
            /// <summary>Номер миксера по отправляемой на сохранение заявке ( "nvoMIXER_NUMBER" )</summary>
            public const string MixerNumber = "nvoMIXER_NUMBER";
            public const string MixerDisplayStatus = "nvoMIXER{0}_DISPLAY_ST";
            /// <summary>nvoVALVE_CMD</summary>
            public const string ValveCommandStatus = "nvoVALVE_CMD";
            public const string SensorStatus = "nvoSENSOR_ST";

            /// <summary>
            /// Время до начала выгрузки из дозатора.
            /// </summary>
            public const string DoserWaitTime = "nvoBATCHER{0}_REMAIN_TIME_START_UNLOAD";

            /// <summary>
            /// Моточасы смесителя
            /// </summary>
            public const string MixerMotoHour = "nvoMIXER{0}_EN_H";

            /// <summary>
            /// Моточасы компрессора
            /// </summary>
            public const string CompressorMotoHour = "nvoSTART{0}_EN_H";

            /// <summary>
            /// Номер заявки на смесителе.
            /// </summary>
            public const string MixerApplicationNumber = "nvoMIXER{0}_REQ_POINT";
            public const string MixerBatchNumber = "nvoMIXER{0}_BATCH_POINT";
            public const string MixerLastBatch = "nvoMIXER{0}_LAST_BATCH";
            /// <summary>Хранит сколько уже отдозировано в смеситель из дозатора (из такого-то бункера)</summary>
            public const string MixerBunkerWeight = "nvoMIXER{0}_BUNKER{1}_WEIGHT";
            public const string MixerBunkerNeedWeight = "nvoMIXER{0}_BUNKER{1}_NEED_WEIGHT";
            public const string MixerMixTime = "nvoMIXER{0}_REMAIN_TIME_MIX";
            public const string MixerUnloadTime = "nvoMIXER{0}_REMAIN_TIME_UNLOAD";
            public const string CurrentConvert = "nvoCUR{0}_CONVERT";
            public const string CurrentNotConvert = "nvoCUR{0}";
            public const string StartCommand = "nvoSTART_CMD";
            public const string StartStatus = "nvoSTART_ST";
            public const string MixerRemainTimeToStart = "nvoMIXER{0}_REMAIN_TIME_START";
            public const string VibrationCommand = "nvoVIBR_CMD";
            public const string ValveStatus = "nvoVALVE_ST";
            /// <summary>
            /// Номер замеса первой заявки в очереди ( "nvoBATCH_NUMBER_{0}" )
            /// </summary>
            public const string PlcBatchNumber = "nvoBATCH_NUMBER_{0}";

            /// <summary>
            /// Статус замеса первой заявки в очереди. "nvoBATCH_ST_{0}"
            /// </summary>
            public const string PlcBatchStatus = "nvoBATCH_ST_{0}";

            /// <summary>
            /// Номер заявки, которая первая в очереди. "nvoREQ_NUMBER_{0}"
            /// </summary>
            public const string PlcApplicationNumber = "nvoREQ_NUMBER_{0}";
            //public const string AccumRemainTimeStartUnload = "nvoACCUM{0}_REMAIN_TIME_START_UNLOAD";
            /// <summary>Время до окончания выгрузки</summary>
            //public const string AccumRemainTimeUnload = "nvoACCUM{0}_REMAIN_TIME_UNLOAD";

            /// <summary>Вес, который уже выпал</summary>
            //public const string AccumBunkerWeight = "nvoACCUM_BUNKER{0}_WEIGHT";

            /// <summary>Номер заявки</summary>
            //public const string AccumApplication = "nvoACCUM{0}_REQ_POINT";
            /// <summary>Номер замеса</summary>
            //public const string AccumBatch = "nvoACCUM{0}_BATCH_POINT";


            public const string SkipRemainTimeStartUnload = "nvoSKIP{0}_REMAIN_TIME_START_UNLOAD";
            /// <summary>Время до окончания выгрузки</summary>
            public const string SkipRemainTimeUnload = "nvoSKIP{0}_REMAIN_TIME_UNLOAD";

            /// <summary>Вес, который уже выпал</summary>
            public const string SkipBunkerWeight = "nvoSKIP{0}_BUNKER{1}_WEIGHT";

            /// <summary>Номер заявки</summary>
            public const string SkipApplication = "nvoSKIP{0}_REQ_POINT";
            /// <summary>Номер замеса</summary>
            public const string SkipBatch = "nvoSKIP{0}_BATCH_POINT";
            /// <summary>Последний замес</summary>
            public const string SkipLastBatch = "nvoSKIP{0}_LAST_BATCH";

            /// <summary>
            /// Фаза работы скипа. Дублирует nci.SkipBatchPhase с добавлением фазы "Движение"
            /// </summary>
            public const string SkipBatchPhase = "nvoSKIP{0}_BATCH_PHASE";

            /// <summary>СВИ {0} - общий счётчик</summary>
            public const string WaterCounterTotalAmount = "nvoWaterCounter{0}TotalAmount";

            /// <summary>Идет промывка</summary>
            public const string WashActive = "nvoWashActive";

            /// <summary>Время промывки вышло: 0й бит -1й дозатор и т.д.</summary>
            public const string TimeWashEnd = "nvoTimeWashEnd";

            /// <summary>Объём воды, налитый за замес.</summary>
            public const string WaterCounterMixerAmount = "nvoWaterCounterMixer{0}Amount";

            /// <summary>Доливка воды в процессе</summary>
            public const string WaterRefillingIsActive = "nvoDWD{0}_InWork";

            /// <summary>Время доливки воды закончилось</summary>
            public const string WaterCounterTimeIsOver = "nvoDWD{0}_TimeIsOver";

            /// <summary>Осталось времени до окончания доливки</summary>
            public const string WaterCounterTimeRemain = "nvoDWD{0}_TimeRemain";

            /// <summary>Статус дозатора о работе в 2 этапа</summary>
            public const string TwoStepDosingStatus = "nvoTwoStepDosing";

            /// <summary>
            /// Фактическая влажность. ( "nvoBatchMoisture" )
            /// (цифра - номер строки в буфере на контроллере). На момент открытия затвора/начала выгрузки</summary>
            public const string BatchMoisture = "nvoBatchMoisture";

            /// <summary>Ток. На момент открытия затвора (начала выгрузки) ( "nvoBatchMixerCurrent" )</summary>
            public const string BatchMixerCurrent = "nvoBatchMixerCurrent";

            /// <summary>Фактическое время перемешивания. ( "nvoBatchActualMixingTime" )</summary>
            public const string BatchActualMixingTime = "nvoBatchActualMixingTime";

            /// <summary>( nvoMixer{0}_Moisture_Not_Normal )</summary>
            public const string MixerMoistureNotNormal = "nvoMixer{0}_Moisture_Not_Normal";

            /// <summary>Сколько будет долито - ( "nvoDWD{0}_Dose_SP" )</summary>
            public const string DwdDoseSetPoint = "nvoDWD{0}_Dose_SP";

            /// <summary>Уникальный код объекта, зарегистрированный на контроллере PLC</summary>
            /// <example>190101(001|002)</example>
            public const string Project_ID = "nvoProject_ID";

            /// <summary>Версия программы</summary>
            public const string ProjectVersion = "nvoProject_Version";

            /// <summary>Оставшееся время до остановки ленты, с</summary>
            public const string LineTimeWait = "nvoLINE{0}_REMAIN_TIME_STOP";

            /// <summary>AI - входная величина ниже диапазона</summary>
            public const string AiErrorBelowRange = "nvoAIErrorBelowRange";

            /// <summary>AI - входная величина выше диапазона</summary>
            public const string AiErrorAboveRange = "nvoAIErrorAboveRange";

            /// <summary>AI - ошибка при масштабировании значения</summary>
            public const string AiErrorCalib = "nvoAIErrorCalib";

            /// <summary>AI - ошибка модуля</summary>
            public const string AiErrorDevice = "nvoAIErrorDevice";

            /// <summary>Объём тек. замеса на смесителе</summary>
            public const string MixerBatchVolume = "nvoMixer{0}_Batch_Volume";

            /// <summary>Ответ на команду калибровки датчика уровня</summary>
            public const string CalibTableStatus = "nvoCalibTableStatus{0}";

            public const string DoserLoadPriority = "nvoBATCHER{0}_LOAD_PR";
            public const string AlarmAccumulatorWeightNalip = "nvoAccum{0}_WeightNalip_alm";

            #region Моточасы

            /// <summary>Моточасы valve</summary>
            public const string ValveMotoHour = "nvoVALVE{0}_EN_H";
            /// <summary>Моточасы vibr</summary>
            public const string VibrMotoHour = "nvoVIBR{0}_EN_H";
            /// <summary>Моточасы start</summary>
            public const string StartMotoHour = "nvoSTART{0}_EN_H";

            #endregion

            /// <summary>Время до начала движения скипа</summary>
            public const string BeforeSkipStart = "nvoBeforeSkip{0}Start";

            /// <summary>Загрузка в смеситель заблокирована по превышению тока </summary>
            public const string CurrentLoadDisabled = "nvoMixer{0}_CurrentLoadDisabled";

            public const string SkipSt = "nvoSKIP{0}_ST";
            public const string AccumSt = "nvoACCUM{0}_ST";

            #region Новая доливка

            /// <summary>коэффициент долива воды по датчику влажности</summary>
            public const string SaveMoistureCoef = "nvoSAVE_MOISTURE_COEF_M{0}";

            /// <summary>требуемая влажность в рецепте</summary>
            public const string SaveYetMixingMoist = "nvoSAVE_YET_MIXING_MOIST_M{0}";

            /// <summary> влажность при сухом перемешивании </summary>
            public const string SaveDryMixing = "nvoSAVE_DRY_MIXING_MOIST_M{0}";

            /// <summary> время сухого перемешивания НЕ ИСПОЛЬЗУЕТСЯ </summary>
            public const string SaveDryMixingTime = "nvoDRY_MIXING_MOIST_M{0}";

            /// <summary> время перемешивания с водой объем этого замеса</summary>
            public const string SaveWetMixingTime = "nvoYET_MIXING_MOIST_M{0}";

            /// <summary>
            /// Рецепт откалиброван по влажности
            /// </summary>
            public const string MoistureCalibrated = "nvoMOISTURE_CALIBRATED_M{0}";


            public const string SAVE_CALIBRATED_RECIPE_VOLUME_SELECT_M = "nvoSAVE_VOLUME_SELECTOR_M{0}";

            public const string SAVE_MOISTURE_STEEP_M = "nvoSAVE_MOISTURE_STEEP_M{0}";

            public const string SAVE_MOISTURE_OFSET_M = "nvoSAVE_MOISTURE_OFSET_M{0}";

            /// <summary>
            /// Влажность заданная по рецепту
            /// </summary>
            public const string TargetMoisture_M = "nvoTargetMoisture_M{0}";

            /// <summary>
            /// требуемое ВЦ для каждого смесителя
            /// </summary>
            public const string TargetWC_M = "nvoTargetWC_M{0}";

            /// <summary>
            /// текущее ВЦ для каждого смесителя
            /// </summary>
            public const string CurrentWC_M = "nvoCurrentWC_M{0}";

            /// <summary>
            /// Рецепт откалиброван
            /// </summary>
            public const string MOISTURE_CALIBRATED_M = "nvoMOISTURE_CALIBRATED_M{0}";

            #endregion  Новая доливка

            #region DryMix
            /// <summary>Оставшееся время до остановки механизма, с <see cref="bool"/></summary>
            public const string ValveRemainTimeStop = "nvoVALVE{0}_REMAIN_TIME_STOP";
            /// <summary>Состояние алгоритма сушки (0-отключен, 1-включен) <see cref="bool"/></summary>
            public const string DryingState = "nvoDryingState";
            /// <summary>команда Стоп поста временной остановки <see cref="bool"/></summary>
            public const string StopPostCMD = "nvoStopPost{0}CMD";
            /// <summary>Время останова</summary>
            public const string RemainTimeStop = "nvoVALVE{0}_REMAIN_TIME_STOP";
            #endregion
            #region Кюбель

            public const string PostSenKubel = "nvoKubel{0}Post{1}Sens1";

            public const string BeforePostSenKubel = "nvoKubel{0}Post{1}Sens2";

            public const string KubelOpened = "nvoKubel{0}Gate1Opened";
            public const string KubelClosed = "nvoKubel{0}Gate1Closed";

            public const string KubelOpenCMD = "nvoKubel{0}Gate1OpenCMD";
            public const string KubelCloseCMD = "nvoKubel{0}Gate1CloseCMD";

            public const string KubelForwardCMD = "nvoKubel{0}ForwardCMD";
            public const string KubelBackwardCMD = "nvoKubel{0}BackwardCMD";

            public const string KubelVibrCMD = "nvoKubel{0}VibrCMD";

            public const string KubelCurrentPost = "nvoKubel{0}CalculatedPos";
            public const string KubelTargetPost = "nvoKubel{0}TargetPost";
            public const string KubelUnloadingPostNumber = "nvoKubel{0}UnloadingPostNumber";

            public const string KubelReqNumber = "nvoKubel{0}ReqNumber";
            public const string KubelBatchNumber = "nvoKubel{0}BatchNumber";

            public const string KubelManualMode = "nvoKubel{0}ManualMode";
            public const string KubelSpeed1 = "nvoKubel{0}Speed1CMD";
            public const string KubelSpeed2 = "nvoKubel{0}Speed2CMD";

            public const string KubelManualForwardCMD = "nvoKubel{0}ManualForwardCMD";
            public const string KubelManualBackwardCMD = "nvoKubel{0}ManualBackwardCMD";
            public const string KubelManualOpenGateCMD = "nvoKubel{0}ManualOpenGateCMD";
            public const string KubelManualCloseGateCMD = "nvoKubel{0}ManualCloseGateCMD";
            public const string KubelPultUnloadCMD = "nvoKubel{0}PultUnloadCMD";


            public const string KubelFull = "nvoKubel{0}Full";
            public const string KubelUnderMixer = "nvoKubelUnderMixer{0}";

            public const string KubelPanelReturnCMD = "nvoKubel{0}Panel{1}ReturnCMD";
            public const string nvoKubelPanelUnloadCMD = "nvoKubel{0}Panel{1}UnloadCMD";

            public const string KubelAlgStatus = "nvoKubel{0}AlgStatus";

            public const string KubelEnableUnload = "nvoKubel{0}EnableUnload";

            public const string KubelEnableReturn = "nvoKubel{0}EnableReturn";

            public const string KubelSpeedCMD = "nvoKubel{0}Speed{1}CMD";

            public const string KubelMovingEngineConnected = "nvoKubel{0}MovingEngineConnected";

            public const string KubelRemainUnload = "nvoKubel{0}RemainUnload";
            #endregion

        }
    }
}
