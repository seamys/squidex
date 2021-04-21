/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { BooleanFieldPropertiesDto, FieldDto, hasNoValue$, LanguageDto } from '@app/shared';
import { Observable } from 'rxjs';

@Component({
    selector: 'sqx-boolean-validation',
    styleUrls: ['boolean-validation.component.scss'],
    templateUrl: 'boolean-validation.component.html'
})
export class BooleanValidationComponent implements OnInit {
    @Input()
    public fieldForm: FormGroup;

    @Input()
    public field: FieldDto;

    @Input()
    public properties: BooleanFieldPropertiesDto;

    @Input()
    public languages: ReadonlyArray<LanguageDto>;

    @Input()
    public isLocalizable?: boolean | null;

    public showDefaultValue: Observable<boolean>;

    public ngOnInit() {
        this.fieldForm.setControl('defaultValue',
            new FormControl());

        this.fieldForm.setControl('defaultValues',
            new FormControl());

        this.fieldForm.setControl('inlineEditable',
            new FormControl());

        this.showDefaultValue =
            hasNoValue$(this.fieldForm.controls['isRequired']);

        this.fieldForm.patchValue(this.properties);
    }
}