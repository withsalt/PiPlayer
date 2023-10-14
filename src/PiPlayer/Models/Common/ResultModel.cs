using PiPlayer.Models.Common.JsonObject;
using PiPlayer.Utils;

namespace PiPlayer.Models.Common
{
    public class ResultModel<T> : IRoot<T> where T : IChild
    {
        public ResultModel()
        {

        }

        public ResultModel(int code)
        {
            Code = code;
            if (code == 0)
            {
                Message = "Success";
            }
        }

        public ResultModel(int code, string message)
        {
            Code = code;
            Message = message;
        }

        public int Code { get; set; }

        public string Message { get; set; }

        public T Data { get; set; }

        public long Time
        {
            get
            {
                return TimeUtil.Timestamp();
            }
        }
    }
}
