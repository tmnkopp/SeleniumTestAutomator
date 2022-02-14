namespace CyberScope.Tests.Selenium
{
    public class InitializeAttempt
    {

        #region CTOR

        public InitializeAttempt()
        {
        } 
        #endregion

        #region PROPS 
        public string Section { get; set; } 
        public object[] GetAsRow => new object[] { Section };

        #endregion

    }
}
