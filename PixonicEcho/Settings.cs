namespace PixonicEcho
{
    class Settings
    {
        /// �������� �������� ��������:
        ///  - ���������� ��� ��� ������� ������� ����� �����:
        /// ClientMessageIntervalMs = 1000
        /// ClientsPerRoom = 2
        /// Rooms = 1
        /// PrintMessagesToConsole = true
        /// ��������� � ������ benchmark. ����� ������� ���� ������� � ��� �������, ������
        /// �� ��������� ����� ���������� �� �������
        /// 
        ///  - ��������� ����������������� �������
        /// ClientMessageIntervalMs = 10
        /// ClientsPerRoom = 10
        /// Rooms = 10
        /// PrintMessagesToConsole = false
        /// ����� ������� 10 ������ � ������ 10 ��������.


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