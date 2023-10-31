using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOffice.Authorizer.Management.Domains
{
    public static class AuthorizerMessageBuilder
    {
        /// <summary>
        /// Método que retorna uma string no formato XML com os parametros para o autorizador com os valores padroões de alguns parâmetros.
        /// </summary>
        /// <param name="parameters"> string com os demais parâmetros</param>
        /// <returns></returns>
        public static string BuildDefaultMessage(string parameters)
        {
            string message = string.Format(CultureInfo.InvariantCulture,"<ROOT>{0}<XTI>1234</XTI><DT4>{1}</DT4><HT6>{2}</HT6></ROOT>", parameters, DateTime.Now.ToString("MMdd"), DateTime.Now.ToString("HHmmss"));

            return message;
        }


        /// <summary>
        /// Método que retorna uma string no formato XML com os parametros para o autorizador com os valores padroões de alguns parâmetros.
        /// </summary>
        /// <param name="parameters"> Dicionário com o nome dos parametros da mensagem e seus valores</param>
        /// <returns>mensagem default</returns>
        public static string BuildDefaultMessage(Dictionary<string, object> parameters)
        {
            string message = "<ROOT>";

            foreach (var parameter in parameters)
            {
                message += String.Format(CultureInfo.InvariantCulture, "<{0}>{1}</{2}>", parameter.Key, parameter.Value, parameter.Key);
            }

            message += "<XTI>1234</XTI>";
            message += $"<DT4>{DateTime.Now.ToString("MMdd")}</DT4>";
            message += $"<HT6>{ DateTime.Now.ToString("HHmmss")}</HT6>";
            message += "</ROOT>";

            return message;
        }

        /// <summary>
        /// Método que retorna uma string no formato XML com os parametros para o autorizador.
        /// </summary>
        /// <param name="parameters"> Dicionário com o nome dos parametros da mensagem e seus valores e pode alterar os valores default dos parâmetros</param>
        /// <returns></returns>
        public static string BuildCustomMessage(Dictionary<string,object> parameters)
        {
            string message = "<ROOT>";

            if (!parameters.ContainsKey("XTI"))
            {
                message += "<XTI>1234</XTI>";
            }

            if (!parameters.ContainsKey("DT4"))
            {
                message += $"<DT4>{DateTime.Now.ToString("MMdd")}</DT4>";
            }

            if (!parameters.ContainsKey("HT6"))
            {
                message += $"<HT6>{ DateTime.Now.ToString("HHmmss")}</HT6>";
            }

            foreach(var parameter in parameters)
            {
               message += String.Format(CultureInfo.InvariantCulture, "<{0}>{1}</{2}", parameter.Key, parameter.Value, parameter.Key);
            }

            message += "</ROOT>";

            return message;
        }
    }
}
