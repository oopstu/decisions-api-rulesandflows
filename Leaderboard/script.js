$(document).ready(function() {
    console.log("Document Loaded!");
    //$('#datetimepicker1').datetimepicker();

      //let city = $('#location').val();
      //$('#location').val("");

    //BEGIN POPULATING PLAYER TABLE
    let request = new XMLHttpRequest();
    let url = `http://localhost/decisions/Primary/?ReportId=a2fb5afb-11c1-11e8-97ae-9cb6d0d26d62&Action=api&userid=admin%40decisions.com&password=admin&outputtype=JSON`;

    request.onreadystatechange = function() {
      if (this.readyState === 4 && this.status === 200) {
          let response = this.responseText
          console.log(response);
          let parsed = JSON.parse(response);
          console.log(parsed.Rows);
          makeRows(parsed.Rows);
          playerDropdown(parsed.Rows);
      }
    }

    request.open("GET", url, true);
    request.send();

    makeRows = function(response) {
      console.log(response.length);
      for (var i = 0; i < response.length; i++) {
          $('#PlayerTable tbody').append('<tr><td>' + response[i].flow_inline_field + '</td>' + '<td>' + response[i].name + '</td>' + '<td>' + Math.trunc(response[i].first) + '</td>' + '<td>' + Math.trunc(response[i].second) + '</td>' + '<td>' + Math.trunc(response[i].third) + '</td>' + '<td>' + Math.trunc(response[i].fourth) + '</td>' + '</tr>');
      }
    }
    //END POPULATING PLAYER TABLE

    //BEGIN ADD PLAYER FORM
    $('#add-player-button').click(function(e) { 
      //e.preventDefault();  //prevent form from submitting
      //console.log("Adding Player!");
      $("#add-player-feedback").empty()
      var playerName = $('#player-name').val();
      if (playerName === "") {
        $('<p class = "alert-danger">Must add player name.</p>').appendTo('#add-player-feedback');
      } else {
        var url = "http://localhost/decisions/Primary/?FlowId=95ae17a9-11d2-11e8-97ae-9cb6d0d26d62&Action=api";
        let parameters = JSON.stringify({
          "userid": "admin@decisions.com",
          "password": "admin",
          "outputtype": "Json",
          "Name": playerName
        });
        $.ajax({
          type: "POST",
          url: url,
          data: parameters,
          dataType: "json",
          success: function(msg, status, jqXHR){
            console.log(msg.Done);
            let exp_resp = "Player " + playerName + " has been created!" 
            //if (msg.Done.response === exp_resp) {
              $('<p class = "alert-success">Player added!</p>').appendTo('#add-player-feedback');
              //window.setTimeout( 5000 );
              $('#addPlayerModal').delay(8000).modal('hide');
              $("#add-player-form")[0].reset();
              $('<p class = "alert-success"></p>').appendTo('#add-player-feedback');
           // } else {
              //$('<p class = "alert-danger">'+ msg.Done.response + '</p>').appendTo('#add-player-feedback');
            //}
          },
          error: function(msg, status, jqXHR){
            let error = JSON.stringify(msg);
            console.log(msg);
            $('<p class = "alert-danger">'+'</p>').appendTo('#add-player-feedback');
          }
        });
      }
      console.log(playerName); //use the console for debugging, F12 in Chrome, not alerts
    });
    //END ADD PLAYER

    //BEGIN ADD RECORD 
    $("select").on("click", function() {
    });
    playerDropdown = function(response){
      console.log("Player Dropdown!");
      var $dropdown = $(".player");
      $.each(response, function() {
          $dropdown.append($("<option />").val(this.name).text(this.name));
      });
    }


    $('#add-record-button').click(function(e) { 
      //e.preventDefault();  //prevent form from submitting
      //console.log("Adding Player!");
      console.log("Adding Record!");
      $("#add-record-feedback").empty();
      var gameName = $("#game option:selected").text();
      var firstPlace = $('#1player').val();
      var secondPlace = $('#2player').val();
      var thirdPlace = $('#3player').val();
      var fourthPlace = $('#4player').val();
      var datetime = $('#game-date').val();
      console.log(gameName);
      console.log(firstPlace);
      if (gameName === "") {
        $('<p class = "alert-danger">Must add pl name.</p>').appendTo('#add-record-feedback');
      } else {
        var url = "http://localhost/decisions/Primary/?FlowId=d1e959ff-1334-11e8-97af-9cb6d0d26d62&Action=api";
        let parameters = JSON.stringify({
          "userid": "admin@decisions.com",
          "password": "admin",
          "outputtype": "Json",
          "gamename": gameName,
          "firstplace": firstPlace,
          "secondplace": secondPlace,
          "thirdplace": thirdPlace,
          "fourthplace": fourthPlace,
          "datetime": datetime
        });
        $.ajax({
          type: "POST",
          url: url,
          data: parameters,
          dataType: "json",
          success: function(msg, status, jqXHR){
            console.log(msg.Done);
            let exp_resp = "Record for " + gameName + " has been created!";
            $('<p class = "alert-success">' + exp_resp + '</p>').appendTo('#add-record-feedback');
            $('#addRecordModal').modal('hide');
            $("#add-record-form")[0].reset();
            $('<p class = "alert-success">'+'</p>').appendTo('#add-record-feedback');
          },
          error: function(msg, status, jqXHR){
            let error = JSON.stringify(msg);
            console.log(error);
            $('<p class = "alert-danger">'+ error + '</p>').appendTo('#add-record-feedback');
          }
        });
      }
      console.log(gameName);
    });
    //END ADD RECORD

    $("#menu-toggle").click(function(e) {
      e.preventDefault();
      $(".sidebar-sticky").toggleClass("closed-side");
  });



  });