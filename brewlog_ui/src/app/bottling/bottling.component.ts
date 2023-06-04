import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { BottlingService } from '../bl-api';
import { Router } from '@angular/router';

@Component({
  selector: 'app-bottling',
  templateUrl: './bottling.component.html',
  styleUrls: ['./bottling.component.css']
})
export class BottlingComponent implements OnInit {

  bottlingForm = new FormGroup({
    temperature: new FormControl(0, [
      Validators.pattern('^[0-9]{1,2}(.[0-9]{1,2})?$'),
      Validators.min(0.1),
      Validators.required,
    ]),
    co2: new FormControl(0, [
      Validators.pattern('^[0-9]{1,2}(.[0-9]{1,2})?$'),
      Validators.required,
    ]),
  });

  neededPressure?: number;

  constructor(private router: Router, private bottlingService: BottlingService) { }

  ngOnInit(): void {
    this.updateNeccesaryPressure();
  }

  submitBottlingForm(){
    if(this.bottlingForm.valid){
      this.bottlingService.addBottlingValues({
        sessionName: localStorage.getItem('currentBrewSession') ?? '',
        addBottlingValues: {
          co2Volume: this.bottlingForm.controls.co2.value ?? 0,
          temperature: this.bottlingForm.controls.temperature.value ?? 0
        }
      }).subscribe({error: (error) => {
        console.log(error);
      }, complete: () => {
        this.updateNeccesaryPressure();
      }})
    }
  }

  updateNeccesaryPressure(){
    this.bottlingService.getNeededCo2PressureInPsi({
      sessionName: localStorage.getItem('currentBrewSession') ?? ''
    }).subscribe({next: (resp) => {
      this.neededPressure = resp;
    }, error: (error) => {
      console.log(error);
    }})
  }

  brewSessionComplete(){
    this.router.navigate(["/"])
  }

}
