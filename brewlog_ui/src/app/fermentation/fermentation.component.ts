import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { FermentationService } from '../bl-api';
import { Router } from '@angular/router';

@Component({
  selector: 'app-fermentation',
  templateUrl: './fermentation.component.html',
  styleUrls: ['./fermentation.component.css'],
})
export class FermentationComponent implements OnInit {

  fermentationValueForm = new FormGroup({
    temperature: new FormControl(0, [
      Validators.pattern('^[0-9]{1,2}(.[0-9]{1,2})?$'),
      Validators.min(1),
      Validators.required,
    ]),
    sg: new FormControl('', [
      Validators.pattern('^1.([0-9]{3})$'),
      Validators.required,
    ]),
    dateTime: new FormControl(new Date().toISOString().slice(0,16), [Validators.required]),
  });

  fermentationCompleteForm = new FormGroup({
    fg: new FormControl('', [
      Validators.pattern('^1.([0-9]{3})$'),
      Validators.required,
    ]),
  })

  currentAbv?: number;

  constructor(
    private router: Router,
    private fermentationService: FermentationService
  ) {}

  ngOnInit(): void {
    this.updateCurrentAbv();
  }

  submitFermentationValueForm() {
    if(this.fermentationValueForm.valid){
      this.fermentationService.addFermentationValue({
        sessionName: localStorage.getItem('currentBrewSession') ?? '',
        addFermentationValue: { fermentationValue: {
          sg: parseFloat(this.fermentationValueForm.controls.sg.value ?? "0"),
          temp: this.fermentationValueForm.controls.temperature.value ?? NaN,
          valueDateTime: this.fermentationValueForm.controls.dateTime.value ?? new Date().toISOString()
        }}
      }).subscribe({error: (error) => {
        console.log(error);
      }, complete: () => {
        this.updateCurrentAbv();
      }})
    }
  }

  submitFermentationCompleteForm(){
    if(this.fermentationCompleteForm.valid){
      this.fermentationService.fermentationStageComplete({
        sessionName: localStorage.getItem('currentBrewSession') ?? '',
        fermentationStageComplete: {
          fg: parseFloat(this.fermentationCompleteForm.controls.fg.value ?? "0")
        }
      }).subscribe({
        error: (error) => {
          console.log(error);
        },
        complete: () => {
          this.router.navigate(["/bottling"]);
        }
      })
    }
  }

  updateCurrentAbv(){
    this.fermentationService.getCurrentAbv({
      sessionName: localStorage.getItem('currentBrewSession') ?? ''
    }).subscribe({next: (resp: {abv: number}) => {
      this.currentAbv = resp.abv;
    }})
  }
}
