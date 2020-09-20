import { Injectable } from "@angular/core";
import { ApiService } from "../shared/services/api.service";
import { environment } from "../../environments/environment";
import { String } from "typescript-string-operations";

@Injectable({
  providedIn: 'root'
})

export class DashboardtService {

  constructor(private apiService: ApiService) { }

  getPulseRate(deviceId, fromDate, toDate) {
    let route = String.IsNullOrWhiteSpace(fromDate) || String.IsNullOrWhiteSpace(toDate) ? `activity/${deviceId}/history` : `activity/${deviceId}/history/${fromDate}/${toDate}`;

    return this.apiService.get(`${environment.apiBaseUrl}/${route}`).toPromise();
  }

}
