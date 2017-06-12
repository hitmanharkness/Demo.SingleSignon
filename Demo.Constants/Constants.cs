
namespace TripGallery
{
    public class Constants
    {

        public const string TripGalleryAPI = "https://localhost:44315/";

        public const string WebForms = "http://localhost:40301/";
		public const string MVC = "https://localhost:44318/";
        public const string TripGalleryMVCSTSCallback = "https://localhost:44318/stscallback";

        public const string TripGalleryAngular = "https://localhost:44316/";

        public const string ClientSecret = "myrandomclientsecret";

        public const string IssuerUri = "https://sts/identity";
        public const string STSOrigin = "https://localhost:44317";
        public const string TripGallerySTS = STSOrigin + "/identity";
        public const string TripGallerySTSTokenEndpoint = TripGallerySTS + "/connect/token";
        public const string TripGallerySTSAuthorizationEndpoint = TripGallerySTS + "/connect/authorize";
        public const string TripGallerySTSUserInfoEndpoint = TripGallerySTS + "/connect/userinfo";
        public const string TripGallerySTSEndSessionEndpoint = TripGallerySTS + "/connect/endsession";
        public const string TripGallerySTSRevokeTokenEndpoint = TripGallerySTS + "/connect/revocation";


    }

}
