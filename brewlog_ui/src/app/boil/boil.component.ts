import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { BoilService } from '../bl-api';

@Component({
  selector: 'app-boil',
  templateUrl: './boil.component.html',
  styleUrls: ['./boil.component.css'],
})
export class BoilComponent implements OnInit {
  preboilForm = new FormGroup({
    preboilVolume: new FormControl(0, [
      Validators.pattern('^[0-9]{1,2}.?[0-9]{1,2}$'),
      Validators.min(1),
      Validators.required,
    ]),
    preboilSg: new FormControl('', [
      Validators.pattern('^1.([0-9]{3})$'),
      Validators.required,
    ]),
  });

  boilAdjustmentForm = new FormGroup({
    addWater: new FormControl(0, [
      Validators.pattern('^[0-9]{1,2}(.[0-9]{1,2})?$'),
    ]),
    addMinutes: new FormControl(0),
  });

  suggestedSgAdjustment?: string;

  adjustmentType: string = 'water';

  constructor(private router: Router, private boilService: BoilService) {}

  ngOnInit(): void {}

  submitPreboilForm() {
    if (this.preboilForm.valid) {
      this.boilService
        .addPreBoilValues({
          sessionName: localStorage.getItem('currentBrewSession') ?? '',
          addPreBoilValues: {
            liters: this.preboilForm.controls.preboilVolume.value ?? 0,
            sg: parseFloat(this.preboilForm.controls.preboilSg.value ?? '0'),
          },
        })
        .subscribe({
          error: (error) => {
            console.log(error);
          },
          complete: () => {
            this.getSuggestedSgAdjustment();
          },
        });
    }
  }

  getSuggestedSgAdjustment() {
    this.boilService
      .getSuggestedSgAdjustment({
        sessionName: localStorage.getItem('currentBrewSession') ?? '',
      })
      .subscribe({
        next: (resp: { addWater: number; addBoilMinutes: number }) => {
          this.suggestedSgAdjustment =
            'Adjust boil ' +
            (resp.addWater !== 0
              ? `volume with ${resp.addWater} liters of water.`
              : `time with additional ${resp.addBoilMinutes} minutes.`);
        },
        error: (error) => {
          console.log(error);
        },
      });
  }

  submitBoilAdjustmentForm() {
    if (
      this.adjustmentType === 'water' &&
      this.boilAdjustmentForm.controls.addWater.valid &&
      this.boilAdjustmentForm.controls.addWater.value
    ) {
      this.boilService
        .logAddedWater({
          sessionName: localStorage.getItem('currentBrewSession') ?? '',
          addAdditionalBoilWater: {
            litres: this.boilAdjustmentForm.controls.addWater.value,
          },
        })
        .subscribe({
          error: (error) => {
            console.log(error);
          },
        });
    } else if (
      this.adjustmentType === 'time' &&
      this.boilAdjustmentForm.controls.addMinutes.valid &&
      this.boilAdjustmentForm.controls.addMinutes.value
    ) {
      this.boilService
        .logExtendedBoilTime({
          sessionName: localStorage.getItem('currentBrewSession') ?? '',
          addExtendedBoilTime: {
            minutes: this.boilAdjustmentForm.controls.addMinutes.value,
          },
        })
        .subscribe({
          error: (error) => {
            console.log(error);
          },
        });
    }
  }

  boilComplete() {
    this.boilService
      .boilComplete({
        sessionName: localStorage.getItem('currentBrewSession') ?? '',
      })
      .subscribe({
        error: (error) => {
          console.log(error);
        },
        complete: () => {
          this.router.navigate(["/cooling"]);
        }
      });
  }
}
