using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocosBalloon.Core
{

	public static class MeetupConfig
	{
		public static string ApiKey = "";
		public static string MeetupEventId = "214508992";
		public static List<string> CachedImageUrls = new List<string> {
			"http://photos1.meetupstatic.com/photos/member/b/2/7/c/thumb_226365692.jpeg",
			"http://photos1.meetupstatic.com/photos/member/6/d/b/2/thumb_109108082.jpeg",
			"http://photos4.meetupstatic.com/photos/member/b/9/b/6/thumb_144407542.jpeg",
			"http://photos2.meetupstatic.com/photos/member/4/0/3/a/thumb_209476442.jpeg",
			"http://photos2.meetupstatic.com/photos/member/d/f/4/7/thumb_241977159.jpeg",
			"http://photos4.meetupstatic.com/photos/member/b/6/1/e/thumb_230986622.jpeg",
			"http://photos4.meetupstatic.com/photos/member/d/2/3/4/thumb_42653812.jpeg",
			"http://photos3.meetupstatic.com/photos/member/3/1/1/1/thumb_12192561.jpeg",
			"http://photos1.meetupstatic.com/photos/member/3/4/9/6/thumb_212473462.jpeg",
			"http://photos3.meetupstatic.com/photos/member/4/8/c/8/thumb_214578632.jpeg",
			"http://photos2.meetupstatic.com/photos/member/4/a/1/2/thumb_41718962.jpeg",
			"http://photos1.meetupstatic.com/photos/member/a/e/b/4/thumb_242084724.jpeg",
			"http://photos4.meetupstatic.com/photos/member/9/7/7/4/thumb_213218772.jpeg",
			"http://photos4.meetupstatic.com/photos/member/6/a/7/4/thumb_189867252.jpeg",
			"http://photos2.meetupstatic.com/photos/member/4/4/b/8/thumb_234917592.jpeg",
			"http://photos2.meetupstatic.com/photos/member/c/c/e/4/thumb_211552452.jpeg",
			"http://photos1.meetupstatic.com/photos/member/c/4/f/6/thumb_148850422.jpeg",
			"http://photos2.meetupstatic.com/photos/member/3/4/f/a/thumb_189133562.jpeg",
			"http://photos3.meetupstatic.com/photos/member/7/5/1/b/thumb_241589979.jpeg",
			"http://photos2.meetupstatic.com/photos/member/5/e/2/1/thumb_241824097.jpeg",
			"http://photos2.meetupstatic.com/photos/member/6/5/6/thumb_68941622.jpeg",
			"http://photos1.meetupstatic.com/photos/member/5/a/0/2/thumb_212723042.jpeg"
		};
	}

    #region Meetup Classes
    public class Member
    {
        public string name { get; set; }
        public int member_id { get; set; }
    }

    public class MemberPhoto
    {
        public string photo_link { get; set; }
        public string highres_link { get; set; }
        public string thumb_link { get; set; }
        public int photo_id { get; set; }
    }

    public class Event
    {
        public string id { get; set; }
        public object time { get; set; }
        public string event_url { get; set; }
        public string name { get; set; }
    }

    public class Venue
    {
        public int id { get; set; }
        public double lon { get; set; }
        public bool repinned { get; set; }
        public string name { get; set; }
        public string address_1 { get; set; }
        public double lat { get; set; }
        public string country { get; set; }
        public string city { get; set; }
    }

    public class Group
    {
        public int id { get; set; }
        public object created { get; set; }
        public double group_lat { get; set; }
        public double group_lon { get; set; }
        public string join_mode { get; set; }
        public string urlname { get; set; }
    }

    public class Result
    {
        public string response { get; set; }
        public Member member { get; set; }
        public MemberPhoto member_photo { get; set; }
        public object created { get; set; }
        public Event @event { get; set; }
        public object mtime { get; set; }
        public int guests { get; set; }
        public int rsvp_id { get; set; }
        public Venue venue { get; set; }
        public Group group { get; set; }
    }

    public class Meta
    {
        public string lon { get; set; }
        public int count { get; set; }
        public string signed_url { get; set; } 
        public string link { get; set; }
        public string next { get; set; }
        public int total_count { get; set; }
        public string url { get; set; }
        public string id { get; set; }
        public string title { get; set; }
        public long updated { get; set; }
        public string description { get; set; }
        public string method { get; set; }
        public string lat { get; set; }
    }

    public class MeetupRSVPResponse
    {
        public List<Result> results { get; set; }
        public Meta meta { get; set; }
    }
    #endregion
}
