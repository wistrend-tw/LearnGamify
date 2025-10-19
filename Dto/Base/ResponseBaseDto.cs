namespace LearnGamify.ResponseBaseDto
{
    public class Responser
    {
        public bool result { get; set; } = true;
        public int statusCode { get; set; } = 200;
        public string? methodName { get; set; }
        public string? msg { get; set; }
    }

    public class ResponserT<T>
    {
        public bool result { get; set; } = true;
        public int statusCode { get; set; } = 200;
        public string? msg { get; set; }

        public T? data { get; set; }
    }


    //public class ResponserPageT<T> : ResponserT<T>
    //{
    //    /// <summary>總筆數 預設:0</summary>
    //    public int total { get; set; } = 0;
    //}

    //public class Responser_OperatorInfo
    //{
    //    public string? creator { get; set; }
    //    public string? createTime { get; set; }
    //    public string? updater { get; set; }
    //    public string? updateTime { get; set; }
    //    public string? remover { get; set; }
    //    public string? removeTime { get; set; }
    //}

    //public class Response_Base_AccessToken
    //{
    //    public string? memberToken { get; set; }
    //    public string? refreshToken { get; set; }
    //}
}
