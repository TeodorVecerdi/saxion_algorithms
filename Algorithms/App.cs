using System;
using Debug = application.utils.Debug;

namespace application {
    public class App {
        public static void Main(string[] args) {
            try {
                Debug.EnableFileLogger(true);
                var dcg = new AlgorithmsAssignment();
                dcg.Start();
            } catch (Exception e) {
                Debug.LogError($"An error occured: {e.Message}\nYou can contact the developer Teodor Vecerdi (475884@student.saxion.nl).\nStacktrace:\n{e.StackTrace}");
                throw;
            } finally {
                Debug.FinalizeLogger();
            }
        }
    }
}