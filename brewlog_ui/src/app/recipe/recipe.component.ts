import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, FormBuilder, Validators } from '@angular/forms';
import { BeerStyle, RecipeValues, SessionService } from '../bl-api';
import { Router } from '@angular/router';

@Component({
  selector: 'app-recipe',
  templateUrl: './recipe.component.html',
  styleUrls: ['./recipe.component.css']
})
export class RecipeComponent implements OnInit {
  
  public recipeForm = new FormGroup({
    batchName: new FormControl(''),
    targetOg: new FormControl('1.050', [Validators.pattern('^1\.([0-9]{3})$')]),
    targetFg: new FormControl('1.012', [Validators.pattern('^1\.([0-9]{3})$')]),
    targetMashPh: new FormControl('5.2', [Validators.pattern('^[0-9]{1,2}\.?[0-9]{1,2}$')]),
    mashVolume: new FormControl(18,[Validators.pattern('^[0-9]{1,2}\.?[0-9]{1,2}$')]),
    lauterVolume: new FormControl(13, [Validators.pattern('^[0-9]{1,2}\.?[0-9]{1,2}$')]),
    targetVolume: new FormControl(27,[Validators.pattern('^[0-9]{1,2}\.?[0-9]{1,2}$')]),
    mashTime: new FormControl(60, [Validators.pattern('^[0-9]{1,3}$')]),
    boilTime: new FormControl(60, [Validators.pattern('^[0-9]{1,3}$')]),
    beerStyle:new FormControl(0)
  });

  constructor(private formBuilder: FormBuilder, private session: SessionService,private router: Router) { 

  }
  

  ngOnInit(): void {
    this.createRecipeForm();
  }

  submitRecipe(){
    var recipe: RecipeValues = {
      batchName: this.recipeForm.controls.batchName.value ?? "",
      targetOg: parseFloat(this.recipeForm.controls.targetOg.value ?? "0"),
      targetFg: parseFloat(this.recipeForm.controls.targetFg.value ?? "0"),
      targetMashPh: parseFloat(this.recipeForm.controls.targetMashPh.value ?? "0"),
      mashVolume: this.recipeForm.controls.mashVolume.value ?? 0,
      lauterVolume: this.recipeForm.controls.lauterVolume.value ?? 0,
      targetVolume: this.recipeForm.controls.targetVolume.value ?? 0,
      mashTime: this.recipeForm.controls.mashTime.value ?? 0,
      boilTime: this.recipeForm.controls.boilTime.value ?? 0,
      beerStyle: this.recipeForm.controls.beerStyle.value as BeerStyle
    }
    this.session.addSessionRecipe({addSessionRecipe: {recipe: recipe}, sessionName: localStorage.getItem("currentBrewSession") ?? ""})
    .subscribe({next: () => {
      this.router.navigate(["/yeast-starter"]);
    },
    error: error => {
      console.log(error); //TODO Handle error... it gets fluentvalidation errors and more..
    }  
    })
  }

  createRecipeForm(): FormGroup{
    return this.formBuilder.group({
      batchName: new FormControl(""),
      targetOg: new FormControl(1.050),
      targetFg: new FormControl(1.012),
      targetMashPh: new FormControl(5.2),
      mashVolume: new FormControl(18),
      lauterVolume: new FormControl(13),
      targetVolume: new FormControl(27),
      mashTime: new FormControl(60),
      boilTime: new FormControl(60),
      beerStyle:new FormControl(0)
    })
  }

}
