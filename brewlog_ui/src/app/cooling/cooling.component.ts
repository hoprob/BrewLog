import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { CoolingService } from '../bl-api';
import { Router } from '@angular/router';

@Component({
  selector: 'app-cooling',
  templateUrl: './cooling.component.html',
  styleUrls: ['./cooling.component.css'],
})
export class CoolingComponent implements OnInit {
  coolingForm = new FormGroup({
    volumeInFermenter: new FormControl(0, [
      Validators.pattern('^[0-9]{1,2}(.[0-9]{1,2})?$'),
      Validators.min(1),
      Validators.required,
    ]),
    og: new FormControl('', [
      Validators.pattern('^1.([0-9]{3})$'),
      Validators.required,
    ]),
  });

  constructor(private router: Router, private coolingService: CoolingService) {}

  ngOnInit(): void {}

  submitCoolingForm() {
    if (this.coolingForm.valid) {
      this.coolingService
        .logPostCoolingValues({
          sessionName: localStorage.getItem('currentBrewSession') ?? '',
          reportPostCoolingValues: {
            volumeInFermentationVessle:
              this.coolingForm.controls.volumeInFermenter.value ?? 0,
            og: parseFloat(this.coolingForm.controls.og.value ?? '0'),
          },
        })
        .subscribe({
          error: (error) => {
            console.log(error);
          },
          complete: () => {
            this.router.navigate(['/fermentation']);
          },
        });
    }
  }
}
