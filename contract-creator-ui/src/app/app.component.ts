import { Component } from '@angular/core';
import {FormBuilder, Validators} from '@angular/forms';
import { ContractService } from './contract.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
})
export class AppComponent {
  title = 'contract-creator-ui';

  firstFormGroup = this._formBuilder.group({
    seller: ['', Validators.required],
  });
  secondFormGroup = this._formBuilder.group({
    buyer: ['', Validators.required],
  });
  thirdFormGroup = this._formBuilder.group({
    token: ['', Validators.required],
  });
  forthFormGroup = this._formBuilder.group({
    price: [0, Validators.required],
  });

  send() {
    this._contractService.sendContract(
      {
        seller: this.firstFormGroup.value,
        buyer: this.secondFormGroup.value,
        token: this.thirdFormGroup.value,
        price: this.forthFormGroup.value
      });
  }

  constructor(
    private _formBuilder: FormBuilder,
    private _contractService: ContractService) {}
}
