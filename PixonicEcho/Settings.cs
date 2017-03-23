namespace PixonicEcho
{
    class Settings
    {
        /// Типичные варианты настроек:
        ///  - Посмотреть как два клиента говорят между собой:
        /// ClientMessageIntervalMs = 1000
        /// ClientsPerRoom = 2
        /// Rooms = 1
        /// PrintMessagesToConsole = true
        /// Запускать с ключом benchmark. Будет создана одна комната и два клиента, каждое
        /// их сообщение будет выводиться на консоль
        /// 
        ///  - Проверить прозводительность сервера
        /// ClientMessageIntervalMs = 10
        /// ClientsPerRoom = 10
        /// Rooms = 10
        /// PrintMessagesToConsole = false
        /// Будет создано 10 комнат в каждой 10 клиентов.


        public const int ClientMessageIntervalMs = 100;
        public const int RoomLifeTimeoutMs = 5000;
        public const int ClientLifeMs = 500;
        public const int ClientsPerRoom = 5;
        public const int Rooms = 2;
        public const bool PrintMessagesToConsole = false;
        public const bool UseServerSendBuffering = false;

        public const int NetworkPort = 10001;
    }
}