using System;
using System.Collections.Generic;
using System.Text;

namespace YourThunderstoreTeam.util
{
    public static class Utilities
    {
        /// <summary>
        /// Quick and easy way of printing a message to the Lethal Company chat.
        /// </summary>
        /// <param name="message">The message to print to the chat.</param>
        public static void PrintToChat(string message)
        {
            HUDManager.Instance.AddTextToChatOnServer(message);
        }
    }
}
