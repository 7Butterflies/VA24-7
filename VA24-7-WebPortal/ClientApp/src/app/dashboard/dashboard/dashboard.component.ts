import { Component, OnInit } from '@angular/core';
import { SignalRService } from '../../shared/services/signal-r.service';
import { environment } from '../../../environments/environment';
import { SharedService } from '../../shared/services/shared.service';

import * as Highcharts from "highcharts/highstock";
import { Options } from "highcharts/highstock";
import { Subject } from 'rxjs';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {

  Highcharts: typeof Highcharts = Highcharts;
  chartOptions: any;

  constructor(private signalRService: SignalRService, private sharedService: SharedService) { }

  promises: Promise<any>[] = [];

  pulseRate: Subject<any> = new Subject<any>();

  loggedInUserB2CId;

  ngOnInit() {
    this.promises.push(this.getLoggedInUser());

    Promise.all(this.promises).then(_ => {
      this.initializeSignalRIoTHuB().then(_ => {
        alert("signalR IoT is Initialized");

        this.initializeCharOptions();
      });
    })

  }

  initializeCharOptions() {
    let self = this;
    let randomData = function (): any[] {
      var data = [],
        time = (new Date()).getTime(),
        i;

      for (i = -999; i <= 0; i += 1) {
        data.push([
          time + i * 1000,
          Math.round(2 * 100)
        ]);
      }
      return data;
    }
    this.chartOptions = {
      chart: {
        events: {
          load: function () {

            // set up the updating of the chart each second
            let series = this.series[0];
            self.pulseRate.subscribe((p) => {
              let x = (new Date()).getTime(), // current time
                y = p.Pulserate;
              series.addPoint([x, y], true, true);
            })
            //setInterval(function () {
            //  let x = (new Date()).getTime(), // current time
            //    y = Math.round(Math.random() * 100);
            //  series.addPoint([x, y], true, true);
            //}, 1000);
          }
        }
      },

      time: {
        useUTC: false
      },

      rangeSelector: {
        buttons: [{
          count: 1,
          type: 'minute',
          text: '1M'
        }, {
          count: 5,
          type: 'minute',
          text: '5M'
        }, {
          type: 'all',
          text: 'All'
        }],
        inputEnabled: false,
        selected: 0
      },

      title: {
        text: 'Live random data'
      },

      exporting: {
        enabled: false
      },

      series: [
        {
          name: 'Random data',
          data: randomData()
        }
      ]
    };
  }

  loggedInUser;
  getLoggedInUser() {
    return this.sharedService.getOrCreateLoggedInUser().then((res) => {
      this.loggedInUser = res;
      this.loggedInUserB2CId = res.b2CObjectId
    })
  }

  initializeSignalRIoTHuB() {

    let id = `${this.loggedInUserB2CId}-device-02`;
    id = "device-02";

   return this.signalRService.init(id, environment.signalrIoTHub).then((hubConnection) => {
      hubConnection.start().then(() => {

      })
        .catch(error => console.error(error));

      hubConnection.onclose((error) => {
        hubConnection.start();
        console.error(`Something went wrong: ${error}`);
      });

      hubConnection.on('iotActivitiy', data => {
        console.log(JSON.stringify(data));
        this.pulseRate.next(data);
      });

    });
  }

}
