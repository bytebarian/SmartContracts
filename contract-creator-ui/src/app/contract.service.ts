import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class ContractService {

  constructor(private http: HttpClient) { }

  sendContract(contract : any) : void {
    this.http.post("http://localhost:32769/api/Contract/Create", contract);
  }
}
