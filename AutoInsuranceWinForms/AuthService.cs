using System.Configuration;

namespace AutoInsuranceWinForms
{
    public static class AuthService
    {
        public static UserAccount Authenticate(string email)
        {
            email = (email ?? string.Empty).Trim().ToLowerInvariant();
            if (email.Length == 0) return null;

            if (email == Read("SeniorAgentEmail") || email == Read("AdminEmail"))
                return new UserAccount { Email = email, FullName = "Старший агент", Role = UserRole.Administrator };
            if (email == Read("HeadEmail"))
                return new UserAccount { Email = email, FullName = "Руководитель отдела", Role = UserRole.Administrator };
            if (email == Read("ManagerEmail"))
                return new UserAccount { Email = email, FullName = "Менеджер по страхованию", Role = UserRole.Manager };
            if (email == Read("InsuranceAgentEmail") || email == Read("AdjusterEmail"))
                return new UserAccount { Email = email, FullName = "Страховой агент", Role = UserRole.Adjuster };

            return null;
        }

        private static string Read(string key)
        {
            return (ConfigurationManager.AppSettings[key] ?? string.Empty).Trim().ToLowerInvariant();
        }
    }
}
