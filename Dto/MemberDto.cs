namespace LearnGamify.MemberDto
{
    public class RequestDto
    {
        public class Create
        {
            public string name { get; set; }
        }

        public class ReadList
        {
            public long? id { get; set; }
        }

        public class GetAuth
        {
            public string id { get; set; }
            public string role { get; set; }
        }

        public class Update
        {
            public long id { get; set; }
            public string name { get; set; }
        }


        public class Delete
        {
            public long id { get; set; }
        }

    }

    public class ResponseDto
    {
        public class ReadList
        {
            public long id { get; set; }
            public string? name { get; set; }
        }


    }

}
