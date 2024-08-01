namespace OtokarComm.Model
{
    public class MulticastDTO
    {
        public List<Device> _devices { get; set; }
        public string _message { get; set; }

        public MulticastDTO(List<Device> devices, string message)
        {
            this._devices= devices;
            this._message = message;
        }
    }
}
