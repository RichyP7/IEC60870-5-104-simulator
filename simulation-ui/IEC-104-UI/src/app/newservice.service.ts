import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { DataPoint, DataPointsService } from 'iec60870-104-simulator';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class NewserviceService extends DataPointsService {

  httpall = inject(HttpClient);
  override apiEndpoint: String = "http://localhost:8090/api/";
  override healthEndpoint: String = "http://localhost:8090/health/";
constructor(){
    console.log("constructor NewserviceService");
    super();
  }

  override fetchData(): Observable<DataPoint[]> {
    console.log(`test overload +${this.apiEndpoint}`);
    return super.fetchData();
  }
  override fetchHealthState(): Observable<String> {
    console.log(`test overload +${this.apiEndpoint}`);
    return super.fetchHealthState();
  }
}

