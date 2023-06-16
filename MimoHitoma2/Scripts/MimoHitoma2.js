//For checking if a string is empty, null or undefined:
function isEmpty(str) {
    return (!str || 0 === str.length);
}
//For checking if a string is blank, null or undefined:
function isBlank(str) {
    return (!str || /^\s*$/.test(str));
}
//For checking if a string is blank or contains only white-space:
String.prototype.isEmpty = function() {
    return (this.length === 0 || !this.trim());
};


function SearchBar_OnKeyUp()
{
    var inputStr = $("#input-tag-frame").val();
    var frontWords = "";
    var lastWordPrefix = "";
    var words = inputStr.split(", ");
    
    if (words.length < 1)
    {
        frontWords = "";
        lastWordPrefix = inputStr.trim();
    }
    else
    {
        for (i = 0; i < words.length - 1; i++) {
            frontWords += words[i].trim() + ", ";
        }
        lastWordPrefix = words[words.length - 1].trim();
    }
            
    if (!(isEmpty(lastWordPrefix) || isBlank(lastWordPrefix)))
    {
        $("#input-tag-frame").autocomplete({
            source: function (request, response) {
                $.ajax({
                    url: "/MimoHitoma2/AutoComplete/Tags/",
                    type: "POST",
                    dataType: "json",
                    data: { Prefix: lastWordPrefix },
                    success: function (data) {
                        response($.map(data, function (item) {
                            return { label: item, value: frontWords + item }; // 配合inputStr.split的條件，加入個空白，使其拼湊後與原先輸入字串相同
                        }))
                    }
                })
            },
            messages: {
                noResults: "", results: ""
            }
        });      
    }
}

function searchBarSubmit()
{
    var text = $("#input-tag-frame").val();
    // 將分隔tags的逗號(',')，轉為URL接受型態的加號('+')
    var words = text.split(",");
    var tagsURLFormat = "";
    for (i = 0 ; i < words.length - 1 ; i++)
        tagsURLFormat += words[i].trim() + "+";
    tagsURLFormat += words[words.length - 1].trim();

    location.href = "/MimoHitoma2/Post/Search?tags=" + tagsURLFormat;
    // 由於Form的Submit優先於javascript的location.href，因此要取消該form的submit event
    return false; 
}

function redirectPageSubmit()
{
    var redirectPage = $("#redirect-page-frame").val();
    if (location.href.indexOf("?") == -1)
    {
        var redirectURL = "/MimoHitoma2/Post/Search?&page=" + redirectPage;
        location.href = redirectURL;
    }
    else 
    {
        var currentSearch = location.search.slice(1);
        var parameters = currentSearch.split("&");
        var indexOfPage = -1;
        var newSearch = "";

        if(parameters.length >= 1)
        {
            for(i = 0; i < parameters.length; i++)
            {
                if ((parameters[i] != undefined) && (!isEmpty(parameters[i])))
                {
                    if (parameters[i].toLowerCase().indexOf("page") != -1)
                        indexOfPage = i;
                }
                
            }

            for(j = 0; j < parameters.length; j++)
            {
                if ((parameters[j] != undefined) && (!isEmpty(parameters[j])))
                {
                    if (j != indexOfPage)
                        newSearch += parameters[j] + "&";
                }
            }

            if((newSearch != undefined) && (!isEmpty(newSearch)))
                var redirectURL = "/MimoHitoma2/Post/Search?" + newSearch + "page=" + redirectPage;
            else
                var redirectURL = "/MimoHitoma2/Post/Search?&" + "page=" + redirectPage;

            location.href = redirectURL;
        }
    }
    // 由於Form的Submit優先於javascript的location.href，因此要取消該form的submit event
    return false;
}



