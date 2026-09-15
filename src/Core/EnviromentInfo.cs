using System.Runtime.InteropServices;

 namespace Core; // корінний namespace = ім'я проєкту Core

 public sealed record EnvironmentReport(
    string OsDescription,
    string FrameworkDescription,
    string ProcessArchitecture,
    string DetectedRid,
    string ReportedRid,
    string BaseDirectory,
    string BuildNote);
 public static class EnvironmentInfo {
    public static EnvironmentReport Collect() => new(

    RuntimeInformation.OSDescription,
    RuntimeInformation.FrameworkDescription,
    RuntimeInformation.ProcessArchitecture.ToString(),

    DetectRid(),

    RuntimeInformation.RuntimeIdentifier,
    AppContext.BaseDirectory,
    
    BuildInfo()
    );
    
      private static string BuildInfo()
      {
         #if NET9_0_OR_GREATER
            return "збірка під net9.0";
         
         #else
            return "збірка під net8.0";

         #endif
      }

    // Ручне визначення RID: показує, з чого складається рядок win-x64.
     private static string DetectRid() {
        string os =
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "win" :
        RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "linux" :
        RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "osx" : "unknown";

        string arch = RuntimeInformation.ProcessArchitecture switch {
            Architecture.X64 => "x64",
            Architecture.X86 => "x86",
            Architecture.Arm64 => "arm64",
            Architecture.Arm => "arm",
            _ => "unknown"
        };

        return $"{os}-{arch}";
    }
 }