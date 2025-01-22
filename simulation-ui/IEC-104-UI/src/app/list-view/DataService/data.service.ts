import { Injectable } from '@angular/core';
import {BehaviorSubject, Observable} from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { DataPoint} from '../list-view.component';
import {environment} from '../../../environments/environment.development';

@Injectable({
  providedIn: 'root'
})
export class DataService {
  private dataSubject = new BehaviorSubject<DataPoint[]>([]);
  data$ : Observable<DataPoint[]> = this.dataSubject.asObservable();

  constructor(private http: HttpClient) {}

  fetchData(): void {
    this.http.get<DataPoint[]>(environment.apiUrl + 'DataPointConfigs').subscribe((data) => {
      this.dataSubject.next(data);
    });
  }

}
