
namespace GeekBoy.Observer
{
    public class NotifyData
    {
        public NotifyData(string type, object data)
        {
            this.Type = type;
            this.Data = data;
        }

        public string Type { get; set; }
        public object Data { get; set; }
    }
}
