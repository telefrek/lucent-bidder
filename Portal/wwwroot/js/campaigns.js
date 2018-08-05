const connection = new signalR.HubConnectionBuilder()
    .withUrl("/campaignHub")
    .build();

connection.on("CampaignUpdate", (message) => {
    var jObj = JSON.parse(message);
    if (jObj.id) {
        var cElem = document.getElementById(jObj.id);
        if (cElem) {
            var cSpend = document.getElementById(jObj.id + '-spend');
            if (cSpend && jObj.spend) {
                cSpend.innerText = jObj.spend;
            }

            var cName = document.getElementById(jObj.id + '-name');
            if (cName && jObj.name) {
                cName.innerText = jObj.name;
            }
        } else {
            location.reload(); // reload for the new campaign
        }
    }
});

connection.start().catch(err => console.error(err.toString()));