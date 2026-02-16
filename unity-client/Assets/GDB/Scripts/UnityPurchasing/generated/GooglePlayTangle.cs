// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("MH72pvOmqLvZXLvHx6IqM0r67X4rOWCBwUpuCiLLutZgFxFQGrrUQgCDgUta6Gvz1vteZDWDNAHSa8o77lzf/O7T2Nf0WJZYKdPf39/b3t0j1xjxzQM+a3DacNuuRZKVXH/kyFzf0d7uXN/U3Fzf395qZxGbETTFgegHOwgGEc46+6qGAodEvBtcE2ePXPI5i0wdTikt/BRiUczA7ByLPNjSkLKv2Hgob9jdspg23Q5itAFZcHv9k7QUummxxN0tez0KkQa+SQ1n8e46FCgtIT9bIYAnal0X0kap4h8p9XSk2FW4P8okmOhJf+vuuqki61HtmNmByTTtQ02e0dSS396C4JXlCrliRtnA3W5+zM5RbOkHUX7KpTXtj7gvbwJmEdzd397f");
        private static int[] order = new int[] { 7,5,4,7,11,5,8,8,9,11,13,12,13,13,14 };
        private static int key = 222;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
