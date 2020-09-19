import { Component, OnInit } from '@angular/core';
import { SignalRService } from '../../shared/services/signal-r.service';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {

  constructor(private signalRService: SignalRService) { }

  ngOnInit() {
  }

}
