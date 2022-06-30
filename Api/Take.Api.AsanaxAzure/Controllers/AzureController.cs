using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json.Linq;

using Take.Api.AsanaxAzure.Models.Azure;

namespace Take.Api.AsanaxAzure.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AzureController : ControllerBase
    {
        [HttpPost("create")]
        public void CreateCardAsana([FromBody] CreatePbiAzureResponse value, [FromHeader] string asanaProjectId)
        {
            var ClienteCs = value.Resource.Fields.ClienteCS;
            if (ClienteCs != "Sodexo")
            {
                Console.WriteLine("Cliente CS não integrado!");
                return;
            }

            List<string> listAcceptedWorkItemTypes = new List<string>
            {
                "Product Backlog Item",
                "Improvement",
                "Incident",
                "Problem",
                "Service Request"
            };

            var ItemType = value.Resource.Fields.ItemType;
            if (!listAcceptedWorkItemTypes.Contains(ItemType))
            {
                Console.WriteLine("Tipo de card não integrado!");
                return;
            }

            // Create card Asana
            using (var wb = new WebClient())
            {
                var url = "https://app.asana.com/api/1.0/tasks?projects=" + asanaProjectId;
                wb.Headers.Add("Authorization", "Bearer 1/1201360402246563:7ca656911d7c6aba7c041b90373bb95a");

                int idAzure = value.Resource.Id;
                string tagsToAsana = convertTagsAsana(value.Resource.Fields.Tags);
                string statusDemanda = setStatusAsana(value.Resource.Fields.State);
                string assignedTo = value.Resource.Fields.AssignedTo;
                string[] emailAssigne = { };

                var data = new NameValueCollection();
                data["name"] = value.Resource.Fields.Title;

                string descHTML = value.Resource.Fields.Description;
                string plainText = ConvertDescription(descHTML);
                Console.WriteLine("Desc: " + plainText);
                data["html_notes"] = "<body>" + plainText + "</body>";

                string comment = (value.Resource.Fields.History);

                string linkAzure = "https://dev.azure.com/felipefalvesdev/Teste%20Intera%C3%A7%C3%A3o%20Azure%20X%20Asana/_boards/board/t/Teste%20Intera%C3%A7%C3%A3o%20Azure%20X%20Asana%20Team/Issues/?workitem=" + idAzure;

                data["tags"] = tagsToAsana;
                data["custom_fields"] = "{\"1201305958621213\":" + "\"" + statusDemanda + "\"" +
                ",\"1202313270152569\":" + "\"" + idAzure + "\"" +
                ",\"1202382619266385\":" + "\"" + linkAzure + "\"}";

                if (assignedTo != "")
                {
                    emailAssigne = assignedTo.Split('<', '>');
                    data["assignee"] = emailAssigne[1];
                }

                var response = wb.UploadValues(url, "POST", data);
                string responseInString = Encoding.UTF8.GetString(response);

                Console.WriteLine("Create Id:" + value.Resource.Id);
            }
        }

        [HttpPost("update")]
        public void UpdateCardAsana([FromBody] UpdatePbiAzureResponse value, [FromHeader] string asanaProjectId)
        {
            // Verify card
            var ClienteCs = value.ResourceUpdate.Revision.Fields.ClienteCS;
            if (ClienteCs != "Sodexo")
            {
                Console.WriteLine("Cliente CS não integrado!");
                return;
            }

            if (value.ResourceUpdate.Relations.AddedRelations != null)
            {
                Console.WriteLine("Subtasks não integradas!");
                return;
            }

            List<string> listAcceptedWorkItemTypes = new List<string>
            {
                "Product Backlog Item",
                "Improvement",
                "Incident",
                "Problem",
                "Service Request"
            };

            var ItemType = value.ResourceUpdate.Revision.Fields.ItemType;
            if (!listAcceptedWorkItemTypes.Contains(ItemType))
            {
                Console.WriteLine("Tipo de card não integrado!");
                return;
            }

            var RevisionItemType = value.ResourceUpdate.RevisionFields.RevisionWorkItemType;
            if (RevisionItemType.newValue != "" && !listAcceptedWorkItemTypes.Contains(RevisionItemType.newValue))
            {
                Console.WriteLine("Tipo de card não integrado!");
                return;
            }
            else if (RevisionItemType.oldValue != "" && !listAcceptedWorkItemTypes.Contains(RevisionItemType.oldValue))
            {
                Console.WriteLine("Tipo de card agora aceito, hora de cria-lo!");
                CreatePbiAzureResponse newCard = new CreatePbiAzureResponse();
                newCard.EventType = value.EventType;
                newCard.Resource.Id = value.ResourceUpdate.Revision.Id;
                newCard.Resource.Fields = value.ResourceUpdate.Revision.Fields;
                CreateCardAsana(newCard, asanaProjectId);
                return;
            }

            // Update card
            int idAzure = (value.ResourceUpdate.Revision.Id);
            string asanaField = (value.ResourceUpdate.Revision.Fields.Asana);

            if (asanaField == null)
            {
                Console.WriteLine("Asana Field is null");
                asanaField = SetLinkAsana(idAzure, asanaField);
            }

            string[] asanaLink = (asanaField).Split('/');
            string asanaId = asanaLink[5];

            string updatedTagsAsana = (value.ResourceUpdate.Revision.Fields.Tags);
            setTagsAsana(updatedTagsAsana, asanaId);
            Console.WriteLine("Tags atualizadas");


            Comment commentInfo = (value.ResourceUpdate.Revision.Comment);
            if (commentInfo != null)
            {
                Console.WriteLine("Tem comentário novo!");
                string commentText = value.ResourceUpdate.Revision.Fields.History;
                setCommentAsana(commentInfo, commentText, asanaId);
            }

            using (var wb = new WebClient())
            {
                var url = "https://app.asana.com/api/1.0/tasks/" + asanaId;
                wb.Headers.Add("Authorization", "Bearer 1/1201360402246563:7ca656911d7c6aba7c041b90373bb95a");

                string statusDemanda = setStatusAsana(value.ResourceUpdate.Revision.Fields.State);
                Console.WriteLine("Status pego");
                string assignedTo = value.ResourceUpdate.Revision.Fields.AssignedTo;
                string[] emailAssigne = { };

                var data = new NameValueCollection();
                data["name"] = value.ResourceUpdate.Revision.Fields.Title;

                var descHTML = value.ResourceUpdate.Revision.Fields.Description;
                var plainText = ConvertDescription(descHTML);
                data["html_notes"] = "<body>" + plainText + "</body>";

                data["custom_fields"] = "{\"1201305958621213\":" + "\"" + statusDemanda + "\"}";

                if (assignedTo != "")
                {
                    emailAssigne = assignedTo.Split('<', '>');
                    data["assignee"] = emailAssigne[1];
                }

                var response = wb.UploadValues(url, "PUT", data);
                string responseInString = Encoding.UTF8.GetString(response);

                Console.WriteLine("Update Id:" + value.ResourceUpdate.Revision.Id);
            }
            Console.WriteLine("Card atualizado com sucesso!");
        }

        string ConvertDescription(string descHTML)
        {
            var auxText = Regex.Replace(descHTML, "<img(.)*?>", "(imagem aqui)\n");
            auxText = Regex.Replace(auxText, "<[/]((div)|(li))*?>", "\n");
            auxText = Regex.Replace(auxText, "<li>", "- ");
            auxText = Regex.Replace(auxText, "&nbsp;", " ");
            auxText = Regex.Replace(auxText, "<span>&#(\\d)*?;</span>", "(emoji aqui)");

            auxText = Regex.Replace(auxText, "<(div|span|/span|li|ul|/ul|br|\n)*?>", "");
            var plainText = Regex.Replace(auxText, "<(div.*?|span.*?|li.*?|ul.*?|br.*?)*?>", "");

            string pattern = "src=\\\"http(.)*?(.png|.jpg|.jpeg)";
            Regex rg = new Regex(pattern);
            MatchCollection matchedImgs = rg.Matches(descHTML);
            string novaString = "";
            if (matchedImgs.Count > 0)
            {
                Regex regex = new Regex("\\(imagem aqui\\)");
                string[] substrings = regex.Split(plainText);
                for (int i = 0; i < substrings.Length; i++)
                {
                    if (matchedImgs.Count > i)
                    {
                        novaString += substrings[i] + "(<a href=\"" + matchedImgs[i].Groups[0].Value + "\">Imagem " + (i + 1) + "</a>)";
                    }
                    else
                    {
                        novaString += substrings[i];
                    }
                }

                plainText = novaString;
            }

            plainText = Regex.Replace(plainText, "src=\\\"", "");
            plainText = Regex.Replace(plainText, "\\n \\n \\n (\\n )*", "\n");
            return plainText;
        }

        string SetLinkAsana(int idAzure, string linkAsana)
        {
            // Get Asana link
            using (var wb = new WebClient())
            {
                var url = "https://app.asana.com/api/1.0/workspaces/15292640478948/tasks/search?custom_fields.1202313270152569.value=" + idAzure;
                wb.Headers.Add("Authorization", "Bearer 1/1201360402246563:7ca656911d7c6aba7c041b90373bb95a");

                var response = wb.DownloadData(url);
                string responseInString = Encoding.UTF8.GetString(response);

                var jsonObj = JObject.Parse(responseInString);
                var dataObj = jsonObj.SelectToken("data");
                JArray dataArray = (JArray)dataObj;

                var idObj = JObject.Parse(dataArray[0].ToString());
                string idAsana = idObj.SelectToken("gid").ToString();

                linkAsana = "https://app.asana.com/0/1201626122921923/" + idAsana;
            }

            // Update Azure Card with Asana link
            using (var wb = new WebClient())
            {
                var urlAzure = "https://dev.azure.com/felipefalvesdev/Teste%20Interação%20Azure%20X%20Asana/_apis/wit/workitems/" + idAzure + "?api-version=6.0";
                wb.Headers.Add("Content-Type", "application/json-patch+json");

                String username = "alvaro";
                String password = "kao4fwub7dtlqvswtiqwc57443giseuzu7k7h3uo4hdtdwcse5xa";
                string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + password));
                wb.Headers[HttpRequestHeader.Authorization] = string.Format("Basic {0}", credentials);

                string data = "[ { \"op\": \"replace\", \"path\": \"/fields/Custom.Asana\", \"value\": \"" + linkAsana + "\" } ]";
                var response = wb.UploadString(urlAzure, "PATCH", data);
                string responseInString = response;
            }

            return linkAsana;
        }

        void setCommentAsana(Comment commentInfo, string commentText, string asanaId)
        {
            bool isDeleted = false;
            if (commentText == "")
            {
                Console.WriteLine("Buscar infos do comment!!");
                // Get comment info
                using (var wb = new WebClient())
                {
                    Console.WriteLine("Url: " + commentInfo.Url);
                    var urlAzure = commentInfo.Url;
                    wb.Headers.Add("Content-Type", "application/json-patch+json");

                    String username = "alvaro";
                    String password = "kao4fwub7dtlqvswtiqwc57443giseuzu7k7h3uo4hdtdwcse5xa";
                    string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + password));
                    wb.Headers[HttpRequestHeader.Authorization] = string.Format("Basic {0}", credentials);

                    var response = wb.DownloadData(urlAzure);
                    string responseInString = Encoding.UTF8.GetString(response);


                    var jsonObj = JObject.Parse(responseInString);
                    commentText = (jsonObj.SelectToken("text")).ToString();

                    var deleted = (jsonObj.SelectToken("isDeleted"));

                    if (deleted != null)
                    {
                        Console.WriteLine("Comentário deletado? " + deleted.ToString());
                        if (deleted.ToString() == "True")
                        {
                            isDeleted = true;
                        }
                    }
                }

            }

            // Verify if comment already exists
            using (var wb = new WebClient())
            {
                var url = "https://app.asana.com/api/1.0/tasks/" + asanaId + "/stories";
                wb.Headers.Add("Authorization", "Bearer 1/1201360402246563:7ca656911d7c6aba7c041b90373bb95a");

                var response = wb.DownloadData(url);
                string responseInString = Encoding.UTF8.GetString(response);

                var jsonObj = JObject.Parse(responseInString);
                var dataObj = jsonObj.SelectToken("data");
                JArray dataArray = (JArray)dataObj;
                bool commentExists = false;
                string asanaCommentId = "";

                for (int i = 0; i < dataArray.Count; i++)
                {
                    var commentObj = JObject.Parse(dataArray[i].ToString());
                    string textComment = commentObj.SelectToken("text").ToString();

                    if (textComment.Contains(commentInfo.Id))
                    {
                        asanaCommentId = commentObj.SelectToken("gid").ToString();
                        commentExists = true;
                        break;
                    }

                }

                if (isDeleted)
                {
                    Console.WriteLine("Hora de deletar comentario!");
                    updateCommentAsana("DELETE", "", "", asanaCommentId);
                    return;
                }

                Console.WriteLine("Comentario puro: " + commentText);
                commentText = ConvertDescription(commentText);
                Console.WriteLine("Comentario convertido: " + commentText);

                if (commentExists)
                {
                    updateCommentAsana("PUT", commentText, commentInfo.Id, asanaCommentId);
                }
                else
                {
                    addCommentAsana(commentText, asanaId, commentInfo.Id);
                }
            }
        }

        void addCommentAsana(string comment, string asanaId, string azureCommentId)
        {
            using (var wb = new WebClient())
            {
                var url = "https://app.asana.com/api/1.0/tasks/" + asanaId + "/stories";
                wb.Headers.Add("Authorization", "Bearer 1/1201360402246563:7ca656911d7c6aba7c041b90373bb95a");

                var data = new NameValueCollection();
                data["html_text"] = "<body>" + comment + "\n[Comentário integrado - ID: " + azureCommentId + "]</body>";

                var response = wb.UploadValues(url, "POST", data);
                string responseInString = Encoding.UTF8.GetString(response);

                Console.WriteLine("Comentario novo adicionado!");
            }
        }

        void updateCommentAsana(string operation, string textComment, string azureCommentId, string asanaCommentId)
        {
            Console.WriteLine("Hora de atualizar comentário!");
            using (var wb = new WebClient())
            {
                var url = "https://app.asana.com/api/1.0/stories/" + asanaCommentId;
                wb.Headers.Add("Authorization", "Bearer 1/1201360402246563:7ca656911d7c6aba7c041b90373bb95a");

                var data = new NameValueCollection();
                data["html_text"] = "<body>" + textComment + "\n[Comentário integrado - ID: " + azureCommentId + "]</body>";

                var response = wb.UploadValues(url, operation, data);
                string responseInString = Encoding.UTF8.GetString(response);

                Console.WriteLine("Comentario atualizado com sucesso!");
            }
        }

        string getCardAsana(string asanaId)
        {
            using (var wb = new WebClient())
            {
                var urlAddTags = "https://app.asana.com/api/1.0/tasks/" + asanaId;
                wb.Headers.Add("Authorization", "Bearer 1/1201360402246563:7ca656911d7c6aba7c041b90373bb95a");

                var response = wb.DownloadData(urlAddTags);
                string responseInString = Encoding.UTF8.GetString(response);

                return responseInString;
            }
        }

        void setTagsAsana(string updatedTagsAsana, string asanaId)
        {
            string infosCardAsana = getCardAsana(asanaId);
            var jsonObj = JObject.Parse(infosCardAsana);
            var dataObj = jsonObj.SelectToken("data");
            JArray oldTagsArray = (JArray)dataObj.SelectToken("tags");
            List<string> listTagsToRemove = new List<string>();

            if (updatedTagsAsana == "")
            {
                Console.WriteLine("Remover todas as tags");
                for (int i = 0; i < oldTagsArray.Count; i++)
                {
                    var tagObj = JObject.Parse(oldTagsArray[i].ToString());
                    string gid = tagObj.SelectToken("gid").ToString();
                    listTagsToRemove.Add(gid);
                }
                removeTagsAsana((listTagsToRemove), asanaId);
            }
            else
            {
                Console.WriteLine("Atualizar tags");
                string updatedTagsToAsana = convertTagsAsana(updatedTagsAsana);
                string[] updatedTagsToAsanaArray = updatedTagsToAsana.Split(',');

                for (int i = 0; i < oldTagsArray.Count; i++)
                {
                    bool tagExists = false;
                    for (int j = 0; j < updatedTagsToAsanaArray.Length; j++)
                    {
                        var tagObj = JObject.Parse(oldTagsArray[i].ToString());
                        string gid = tagObj.SelectToken("gid").ToString();

                        if (updatedTagsToAsanaArray[j] == gid)
                        {
                            tagExists = true;
                            break;
                        }
                    }
                    if (tagExists == false)
                    {
                        var tagObj = JObject.Parse(oldTagsArray[i].ToString());
                        string gid = tagObj.SelectToken("gid").ToString();
                        listTagsToRemove.Add(gid);
                    }
                }
                addTagsAsana((updatedTagsToAsanaArray), asanaId);
                removeTagsAsana((listTagsToRemove), asanaId);
            }
        }

        void addTagsAsana(string[] arrayTags, string asanaId)
        {
            using (var wb = new WebClient())
            {
                var urlAddTags = "https://app.asana.com/api/1.0/tasks/" + asanaId + "/addTag";
                wb.Headers.Add("Authorization", "Bearer 1/1201360402246563:7ca656911d7c6aba7c041b90373bb95a");

                for (int i = 0; i < arrayTags.Length; i++)
                {
                    var dataAddTags = new NameValueCollection();
                    dataAddTags["tag"] = arrayTags[i];

                    var responseAddTags = wb.UploadValues(urlAddTags, "POST", dataAddTags);
                    string responseAddTagsInString = Encoding.UTF8.GetString(responseAddTags);
                }
            }
        }

        void removeTagsAsana(List<string> listTagsToRemove, string asanaId)
        {
            using (var wb = new WebClient())
            {
                var urlRemoveTags = "https://app.asana.com/api/1.0/tasks/" + asanaId + "/removeTag";
                wb.Headers.Add("Authorization", "Bearer 1/1201360402246563:7ca656911d7c6aba7c041b90373bb95a");

                foreach (var tag in listTagsToRemove)
                {
                    var dataRemoveTags = new NameValueCollection();
                    dataRemoveTags["tag"] = tag;

                    var responseRemoveTags = wb.UploadValues(urlRemoveTags, "POST", dataRemoveTags);
                    string responseRemoveTagsInString = Encoding.UTF8.GetString(responseRemoveTags);
                }
            }
        }

        string setStatusAsana(string state)
        {
            switch (state)
            {
                case "New":
                    return "1201305958622253"; // 1. Novo
                case "Prioritized":
                    return "1201305958622265"; // 2. Priorizado
                case "Analyze":
                    return "1201305984480241"; // 3. Em Análise
                case "Ideation":
                    return "1201305984480254"; // 4. Ideação
                case "Ready":
                    return "1201621217553751"; // 5. Pronto para Implementar
                case "Approved":
                    return "1202256353436918"; // 6. Em fila
                case "Commited":
                    return "1201305958622248"; // 7. Em andamento
                case "Test":
                    return "1202256454380267"; // 8. Em Teste
                case "Validation":
                    return "1202256454381336"; // 9. Validação cliente
                case "Done":
                    return "1202256454382396"; // 10. Concluído
                case "Routine":
                    return "1202256454384472"; // Routine
                case "Removed":
                    return "1202256454385529"; // Removed
                case "Active":
                    return "1202256454387743"; // Active
                case "To Do":
                    return "1202256454387774"; // To Do
                case "Refinement":
                    return "1202256454388779"; // Refinement
                case "Deploy":
                    return "1202256454388835"; // Deploy
                case "Metrics":
                    return "1202256454389915"; // Metrics
                case "Reopen":
                    return "1202256454390973"; // Reopen
                case "Impediment":
                    return "1202256454393006"; // Impediment
                case "Design":
                    return "1202256454394096"; // Design
                case "Beta":
                    return "1202256454394127"; // Beta
                default:
                    return "1201305958622253"; // 1. Novo
            }
        }

        string convertTagsAsana(string tags)
        {
            if (tags == "")
            {
                return "";
            }

            tags = Regex.Replace(tags, @"\s+", "");

            string[] arrayTags = tags.Split(';');
            List<string> listTags = new List<string>();
            foreach (var tag in arrayTags)
            {
                switch (tag)
                {
                    case "APIN":
                        listTags.Add("1200015254584399");
                        break;
                    case "DEV":
                        listTags.Add("1200044512192180");
                        break;
                    case "UX":
                        listTags.Add("1200044512192185");
                        break;
                    case "AIC":
                        listTags.Add("1200409121320191");
                        break;
                    case "PM":
                        listTags.Add("1200530067867781");
                        break;
                    case "AGUARDANDOCLIENTE":
                        listTags.Add("1202253484035245");
                        break;
                    case "AGUARDANDODEPLOY":
                        listTags.Add("1202253484035246");
                        break;
                    case "AGUARDANDOPR":
                        listTags.Add("1202253484035247");
                        break;
                    case "AJUSTAR":
                        listTags.Add("1202253484035248");
                        break;
                    case "EMPRODUÇÃO":
                        listTags.Add("1202253484035249");
                        break;
                    case "IMPEDIMENTO":
                        listTags.Add("1202253484035250");
                        break;
                    case "PRIORIDADE":
                        listTags.Add("1202253484035251");
                        break;
                    case "REABERTO":
                        listTags.Add("1202253484035252");
                        break;
                    case "REFINAR":
                        listTags.Add("1202253484035253");
                        break;
                    case "VALIDAÇÃOCLIENTE":
                        listTags.Add("1202253484035254");
                        break;
                }
            }

            string tagsToAsana = "";
            for (int i = 0; i < listTags.Count; i++)
            {
                if (i == 0)
                {
                    tagsToAsana = listTags[0];
                }
                else
                {
                    tagsToAsana = tagsToAsana + "," + listTags[i];
                }
            }

            return tagsToAsana;
        }
    }
}
