/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, ChangeDetectionStrategy, Component, forwardRef, Input, ViewChild } from '@angular/core';
import { ControlValueAccessor, DefaultValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { CodeEditorComponent, Types } from '@app/framework';

export const SQX_FORMATTABLE_INPUT_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => FormattableInputComponent), multi: true
};

type TemplateMode = 'Plain' | 'Script' | 'Liquid';

const MODES: ReadonlyArray<TemplateMode> = ['Plain', 'Script', 'Liquid'];

@Component({
    selector: 'sqx-formattable-input',
    styleUrls: ['./formattable-input.component.scss'],
    templateUrl: './formattable-input.component.html',
    providers: [
        SQX_FORMATTABLE_INPUT_CONTROL_VALUE_ACCESSOR
    ],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class FormattableInputComponent implements ControlValueAccessor, AfterViewInit {
    private fnChanged = (_: any) => { /* NOOP */ };
    private fnTouched = () => { /* NOOP */ };
    private value?: string;

    @Input()
    public type: 'Text' | 'Code';

    @Input()
    public formattable = true;

    @ViewChild(DefaultValueAccessor)
    public inputEditor: DefaultValueAccessor;

    @ViewChild(CodeEditorComponent)
    public codeEditor: CodeEditorComponent;

    public isDisabled: boolean;

    public get valueAccessor(): ControlValueAccessor {
        return this.codeEditor || this.inputEditor;
    }

    public modes = MODES;
    public mode: TemplateMode = 'Plain';

    public ngAfterViewInit() {
        this.valueAccessor.registerOnChange((value: any) => {
            this.value = value;

            this.fnChanged(this.convertValue(value));
        });

        this.valueAccessor.registerOnTouched(() => {
            this.fnTouched();
        });

        this.valueAccessor.writeValue(this.value);
    }

    public writeValue(obj: any) {
        this.mode = 'Plain';

        if (Types.isString(obj)) {
            this.value = obj;

            if (obj.endsWith(')')) {
                const lower = obj.toLowerCase();

                if (lower.startsWith('liquid(')) {
                    this.value = obj.substr(7, obj.length - 8);

                    this.mode = 'Liquid';
                } else if (lower.startsWith('script(')) {
                    this.value = obj.substr(7, obj.length - 8);

                    this.mode = 'Script';
                }
            }
        } else {
            this.value = undefined;
        }

        this.valueAccessor?.writeValue(this.value);
    }

    public setDisabledState(isDisabled: boolean) {
        this.isDisabled = isDisabled;

        this.valueAccessor?.setDisabledState?.(isDisabled);
    }

    public setMode(newMode: TemplateMode) {
        if (newMode !== this.mode) {
            this.mode = newMode;

            this.fnChanged(this.convertValue(this.value));
            this.fnTouched();
        }
    }

    public registerOnChange(fn: any) {
        this.fnChanged = fn;
    }

    public registerOnTouched(fn: any) {
        this.fnTouched = fn;
    }

    private convertValue(value: string | undefined) {
        if (!value) {
            return value;
        }

        value = value.trim();

        switch (this.mode) {
            case 'Liquid': {
                return `Liquid(${value})`;
            }
            case 'Script': {
                return `Script(${value})`;
            }
        }

        return value;
    }
}