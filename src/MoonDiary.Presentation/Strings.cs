namespace MoonDiary.Presentation;

public static class Strings
{
    public const string AppName = "MoonDiary";

    public const string Common_Ok = "Понятно";
    public const string Common_Cancel = "Отмена";
    public const string Common_Delete = "Удалить";
    public const string Common_Edit = "Редактировать";
    public const string Common_Done = "Готово";
    public const string Common_Close = "Закрыть";
    public const string Common_Back = "Назад";
    public const string Common_Error = "Ошибка";
    public const string Common_SaveFailed = "Не удалось сохранить. Введённое осталось на экране, попробуйте ещё раз.";
    public const string Common_LoadFailed = "Не удалось загрузить данные.";
    public const string Common_Energy = "Энергия";
    public const string Common_Positivity = "Позитив";
    public const string Common_NoValue = "—";

    public const string Tab_Calendar = "Календарь";
    public const string Tab_Analytics = "Аналитика";
    public const string Tab_Fill = "Заполнить";
    public const string Tab_Letters = "Письма";
    public const string Tab_User = "Профиль";

    public const string Onboarding_Text = "Каждый вечер — короткая запись о дне. Раз в неделю — отчёт.";
    public const string Onboarding_Question = "Напоминать в 18:00?";
    public const string Onboarding_Allow = "Разрешить уведомления";
    public const string Onboarding_Later = "Не сейчас";

    public const string Calendar_Today = "Сегодня";
    public const string Calendar_Checkin = "Чек-ин";
    public const string Calendar_NoEntry = "нет записи";
    public const string Calendar_Empty = "Записей пока нет. Нажмите «Заполнить», чтобы сделать первую.";
    public const string Calendar_Previous = "Предыдущий месяц";
    public const string Calendar_Next = "Следующий месяц";

    public const string Wizard_Save = "Сохранить";
    public const string Wizard_Continue = "Продолжить";
    public const string Wizard_MoodTitle = "Как ты себя чувствуешь?";
    public const string Wizard_DoneTitle = "Что сделал за день";
    public const string Wizard_AchievementTitle = "Главное достижение";
    public const string Wizard_FeelingsTitle = "Что чувствуешь";
    public const string Wizard_DraftLink = "Есть черновик";
    public const string Wizard_TextPlaceholder = "Можно написать или выбрать блок ниже";
    public const string Wizard_SkipHint = "Смахните, чтобы пропустить";
    public const string Wizard_ChooseScale = "не выбрано";

    public const string Summary_Filled = "заполнено карточек: {0} из 4";

    public const string Checkin_Title = "Чек-ин";
    public const string Checkin_Question = "Что случилось?";
    public const string Checkin_Placeholder = "Коротко, до 500 символов";
    public const string Checkin_DeleteConfirm = "Удалить этот чек-ин?";

    public const string Day_Evening = "Вечерняя запись";
    public const string Day_Checkins = "Чек-ины за день";
    public const string Day_NoEntry = "Записи нет";
    public const string Day_NoCheckins = "Чек-инов нет";
    public const string Day_Fill = "Заполнить";
    public const string Day_FillPast = "Заполнить за этот день";
    public const string Day_Done = "Сделал";
    public const string Day_Achievement = "Достижение";
    public const string Day_Feelings = "Чувства";
    public const string Day_FromCheckins = "Среднее по чек-инам";
    public const string Day_Menu = "Вечерняя запись";
    public const string Day_DeleteConfirm = "Удалить вечернюю запись за этот день? Чек-ины останутся.";

    public const string Report_Title = "Отчёт: {0}";
    public const string Report_Summary = "Итоги недели";
    public const string Report_Achievements = "Достижения";
    public const string Report_Patterns = "Закономерности";
    public const string Report_Comparison = "К прошлой неделе";
    public const string Report_Advice = "Советы";
    public const string Report_NoAdvice = "На этой неделе советов нет.";
    public const string Report_Insufficient = "Заполняйте дневник хотя бы 3 дня в неделю.";
    public const string Report_Useful = "Полезно";
    public const string Report_NotRelevant = "Не актуально";

    public const string Analytics_LastReport = "Последний отчёт";
    public const string Analytics_NoReport = "Отчёт появится в понедельник после первой недели записей.";
    public const string Analytics_OpenReport = "Открыть отчёт";
    public const string Analytics_Dynamics = "Динамика за 4 недели";
    public const string Analytics_NoDynamics = "График появится после первых записей.";
    public const string Analytics_Achievements = "Достижения месяца";
    public const string Analytics_NoAchievements = "В этом месяце достижений пока нет.";
    public const string Analytics_Streak = "Серия";
    public const string Analytics_AllReports = "Все отчёты";
    public const string Reports_Title = "Все отчёты";
    public const string Reports_Empty = "Отчётов пока нет.";

    public const string Letters_Title = "Письма";
    public const string Letters_Empty = "Напишите письмо себе в будущее";
    public const string Letters_New = "+ Новое письмо";
    public const string Letters_WillOpen = "откроется";
    public const string Letters_ClosedTitle = "Письмо закрыто";
    public const string Letters_ClosedMessage = "Оно откроется {0}. Прочитать раньше нельзя.";
    public const string Letters_DeleteConfirm = "Удалить письмо? Восстановить его не получится.";
    public const string NewLetter_Title = "Новое письмо";
    public const string NewLetter_Send = "Отправить";
    public const string NewLetter_Open = "Открыть через";
    public const string NewLetter_WillOpen = "Откроется {0}";
    public const string NewLetter_Time = "Время открытия";
    public const string NewLetter_More = "Позже на день";
    public const string NewLetter_Less = "Раньше на день";
    public const string NewLetter_Placeholder = "Текст письма, до 5000 символов";
    public const string Letter_Written = "Написано {0}";

    public const string User_Name = "Имя";
    public const string User_NamePlaceholder = "Аноним";
    public const string User_Drafts = "Черновики";
    public const string User_Tags = "Блоки-подсказки";
    public const string User_Settings = "Настройки";

    public const string Drafts_Empty = "Черновиков нет";
    public const string Drafts_OnlyMood = "настроение";
    public const string Drafts_DeleteConfirm = "Удалить черновик?";

    public const string Tags_Add = "+ Добавить блок";
    public const string Tags_ArchiveConfirm = "Убрать блок «{0}» из подсказок? В старых записях он останется.";
    public const string Tags_Suggestions = "Часто пишете сами";
    public const string Tags_Accept = "Добавить";
    public const string Tags_Dismiss = "Скрыть";
    public const string Tags_Empty = "В этом наборе блоков нет.";
    public const string Tags_Up = "Выше";
    public const string Tags_Down = "Ниже";
    public const string TagForm_Title = "Новый блок";
    public const string TagForm_Label = "Подпись, до 3 слов";
    public const string TagForm_Icon = "Иконка";
    public const string TagForm_Direction = "Направление жизни";
    public const string TagForm_Save = "Сохранить";

    public const string Settings_Theme = "Тема";
    public const string Settings_ThemeLight = "Светлая";
    public const string Settings_ThemeDark = "Тёмная";
    public const string Settings_ThemeAuto = "Авто";
    public const string Settings_Reminder = "Напоминание";
    public const string Settings_ReminderTime = "Время";
    public const string Settings_Adaptive = "Адаптивное напоминание";
    public const string Settings_AdaptiveHint = "После 14 записей приходит за 10 минут до привычного времени заполнения.";
    public const string Settings_NotificationsDenied = "Уведомления запрещены, напоминания не приходят";
    public const string Settings_Export = "Экспорт данных";
    public const string Settings_ExportButton = "Экспорт";
    public const string Settings_ExportDone = "Файл экспорта сохранён.";
    public const string Settings_Import = "Импорт данных";
    public const string Settings_ImportButton = "Импорт";
    public const string Settings_ImportConfirm = "Текущие данные будут заменены. Перед заменой будет сделан экспорт.";
    public const string Settings_ImportDone = "Данные загружены. Копия прежних данных: {0}";
    public const string Settings_Backup = "Резервная копия Android";
    public const string Settings_BackupOn = "Включена в системе, управляется в настройках Android";
    public const string Settings_BackupOff = "Недоступна на этом устройстве";

    public const string DatabaseError_Title = "База данных не открылась";
    public const string DatabaseError_Text =
        "Записи не удалены. Можно попробовать ещё раз или начать с чистой базы: старый файл будет сохранён рядом.";
    public const string DatabaseError_Retry = "Повторить";
    public const string DatabaseError_Reset = "Начать с чистой базы";
    public const string DatabaseError_ResetConfirm = "Старый файл базы будет переименован и сохранён. Продолжить?";

    public const string Notification_ReminderTitle = "Как прошёл день?";
    public const string Notification_ReminderText = "Запись займёт меньше минуты";
    public const string Notification_LetterTitle = "Письмо из прошлого";
    public const string Notification_LetterText = "Открылось письмо, которое вы написали себе";
}
