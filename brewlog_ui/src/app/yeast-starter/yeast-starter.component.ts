import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { YeastStarter } from '../bl-api/model/yeastStarter';
import { Router } from '@angular/router';
import { YeastStarterService } from '../bl-api';

@Component({
  selector: 'app-yeast-starter',
  templateUrl: './yeast-starter.component.html',
  styleUrls: ['./yeast-starter.component.css'],
})
export class YeastStarterComponent implements OnInit {
  yeastStarters: YeastStarter[] = [];

  packageViability: number = 100;
  packageCells: number = 0;
  totalCells?: number;
  cellsNeeded?: number;

  packageSubmitted: boolean = false;

  public yeastPackageForm = new FormGroup({
    initialCells: new FormControl('100', [Validators.pattern('^[0-9]{1,4}$')]),
    yeastProductionDate: new FormControl('', [Validators.required]),
  });

  constructor(private router: Router, private yeastStarterService: YeastStarterService) {}

  ngOnInit(): void {
    this.getCellsNeeded();
  }

  getCellsNeeded() {
    this.yeastStarterService
      .getYeastCellsNeeded({
        sessionName: localStorage.getItem('currentBrewSession') ?? '',
      })
      .subscribe({
        next: (resp) => {
          this.cellsNeeded = resp;
        },
        error: (error) => {
          console.log(error);
        },
      });
  }

  async submitYeastPackage() {
    if (this.yeastPackageForm.valid) {
      await this.yeastStarterService
        .enterYeastPackageValues({
          sessionName: localStorage.getItem('currentBrewSession') ?? '',
          enterValuesForYeastPackage: {
            yeastProductionDate:
              this.yeastPackageForm.controls.yeastProductionDate.value?.toString(),
            initialYeastCells: parseInt(
              this.yeastPackageForm.controls.initialCells.value ?? '0'
            ),
          },
        })
        .subscribe({
          next: (resp) => {},
          error: (error) => {
            console.log(error);
          },
          complete: () => {
            this.getYeastViability();
          },
        });
    }
  }

  getYeastViability(): boolean {
    this.yeastStarterService
      .getYeastViability({
        sessionName: localStorage.getItem('currentBrewSession') ?? '',
      })
      .subscribe({
        next: (resp) => {
          this.packageCells = resp.calculatedCellsInPackage;
          this.packageViability = resp.viabillityPercentage;
        },
        complete: () => {
          //this.yeastStarters.push(new YeastStarter(this.packageCells));
          this.yeastStarters.push({initialCells: this.packageCells});
          this.packageSubmitted = true;
        },
      });
    return false;
  }

  calculateStarter(starter: YeastStarter) {
    console.log(starter);
    this.yeastStarterService
      .getGramsOfDme({
        sessionName: localStorage.getItem('currentBrewSession') ?? '',
        litresWater: starter.waterLitres ?? 0,
      })
      .subscribe({
        next: (dmeResp) => {
          starter.dryMaltExtract = dmeResp;
          var gramsOfDme = dmeResp;
          console.log(dmeResp);
          this.yeastStarterService
            .getProducedCells({
              gramsOfDME: gramsOfDme,
              initialCells: starter.initialCells ?? 0,
            })
            .subscribe({
              next: (cellsResp) => {
                console.log(cellsResp);
                starter.calculatedYeastCells = cellsResp;
              },
              error: (error) => {
                console.log(error);
              },
            });
        },
        error: (error) => {
          console.log(error);
        },
      });
  }

  addStarter() {
    this.yeastStarters.push(
      {
        initialCells: this.getTotalCells()
      }
    );
    console.log(this.yeastStarters);
  }

  removeStarter() {
    this.yeastStarters.pop();
  }

  saveStarters() {
    this.yeastStarterService.addYeastStarters({
      sessionName: localStorage.getItem('currentBrewSession') ?? '',
      storeYeastStarters: {yeastStarters: this.yeastStarters}
    }).subscribe({next: (resp) => {

    }, error: (error) => {
      console.log(error);
    }, complete: () => {

    }})
  }

  getTotalCells(){
    return this.packageCells + this.yeastStarters.reduce((total, starter) => { return total + (starter.calculatedYeastCells ?? 0)}, 0);
  }

  moveToMash(){
    this.yeastStarterService.yeastStarterComplete({
      sessionName: localStorage.getItem('currentBrewSession') ?? ''
    }).subscribe({
      error: (error) => {
        console.log(error);
      },
      complete: () => {
        this.router.navigate(["/mash"]);
      }
    })
  }
}
