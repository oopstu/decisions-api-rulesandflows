$(document).ready(function() {
    console.log("Document Loaded KART!");
    //$('#datetimepicker1').datetimepicker();

      //let city = $('#location').val();
      //$('#location').val("");

    //BEGIN POPULATING PLAYER TABLE
    let request = new XMLHttpRequest();
    let url = `http://localhost/decisions/Primary/?ReportId=44f9cd17-1359-11e8-97af-9cb6d0d26d62&Action=api&userid=admin%40decisions.com&password=admin&outputtype=JSON`;

    request.onreadystatechange = function() {
      if (this.readyState === 4 && this.status === 200) {
          let response = this.responseText
          let parsed = JSON.parse(response);
          console.log(parsed.Rows);
          makeKartRows(parsed.Rows);
          playerDropdown(parsed.Rows);
      }
    }

    request.open("GET", url, true);
    request.send();

    makeKartRows = function(response) {
      console.log(response.length);
      for (var i = 0; i < response.length; i++) {
          $('#MarioKartTable tbody').append('<tr><td>' + response[i].dateplayed + '</td>' + '<td>' + response[i].gamename + '</td>' + '<td>' + response[i].first + '</td>' + '<td>' + response[i].second + '</td>' + '<td>' + response[i].third + '</td>' + '<td>' + response[i].fourth + '</td>' + '</tr>');
      }
    }
    //END POPULATING KART TABLE

  });