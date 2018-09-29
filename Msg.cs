namespace main
{
    public class Msg
    {
        public long message_id;
        public long peer_id;
        public long timestamp;

        public Msg(long _message_id, long _peer_id, long _timestamp)
        {
            message_id = _message_id;
            peer_id = _peer_id;
            timestamp = _timestamp;
        }
    }
}