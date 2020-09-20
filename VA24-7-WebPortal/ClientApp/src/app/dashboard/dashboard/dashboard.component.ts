import { Component, OnInit } from '@angular/core';
import { SignalRService } from '../../shared/services/signal-r.service';
import { environment } from '../../../environments/environment';
import { SharedService } from '../../shared/services/shared.service';

import * as Highcharts from "highcharts/highstock";
import { Options } from "highcharts/highstock";
import { Subject, BehaviorSubject } from 'rxjs';
import { DashboardtService } from '../dashboard.service';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {

  Highcharts: typeof Highcharts = Highcharts;
  chartOptions: any;

  constructor(private signalRService: SignalRService, private sharedService: SharedService, private dashboardService: DashboardtService) { }

  promises: Promise<any>[] = [];

  pulseRateSubject: BehaviorSubject<any> = new BehaviorSubject<any>(0);

  loggedInUserB2CId;

  ngOnInit() {
    this.promises.push(this.getLoggedInUser());

    Promise.all(this.promises).then(_ => {
      this.initializeSignalRIoTHuB().then(_ => {
        alert("signalR IoT is Initialized");
        this.getContacts();
        this.initializeCharOptions();

        //if (this.loggedInUser.device)
        //  this.getPulseRate(this.loggedInUser.device.deviceId, "", "").then(() => {
        //    this.initializeCharOptions();
        //  })
        //else
        //  this.initializeCharOptions();
      });
    })

  }

  initializeCharOptions() {
    let self = this;
    let preDate = function (): any[] {
      var data = [],
        time = (new Date()).getTime(),
        i;
      for (i = -999; i <= 0; i += 1) {
        data.push([
          time + i * 1000,
          0
        ]);
      }

        data.push(
          [new Date().getTime(), 0]
        )

      return data;
    }
    this.chartOptions = {
      chart: {
        events: {
          load: function () {

            // set up the updating of the chart each second
            let series = this.series[0];
            self.pulseRateSubject.subscribe((p) => {
              let x = (new Date()).getTime(), // current time
                t = new Date(p.PulseDateTime).getTime(),
                y = p.Pulserate;
              series.addPoint([t, y], true, true);
            })
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
          name: 'Pulse data',
          data: preDate()
        }
      ]
    };
  }

  contacts = [];
  getContacts() {
    this.sharedService.getPersons().then((res) => {
      this.contacts = res as [];
      this.contacts = this.contacts.filter(x => x.b2CObjectId != this.loggedInUser.b2CObjectId);
    })
  }

  loggedInUser;
  getLoggedInUser() {
    return this.sharedService.getOrCreateLoggedInUser().then((res) => {
      this.loggedInUser = res;
      this.loggedInUserB2CId = res.b2CObjectId
    })
  }

  pulseData;
  getPulseRate(deviceId, fromDate, Todate) {
    return this.dashboardService.getPulseRate(deviceId, fromDate, Todate).then((res) => {
      this.pulseData = res;
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
        this.pulseRateSubject.next(data);
      });

    });
  }

}
