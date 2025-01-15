"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.JavaLanguageGenerator = void 0;
var _language = require("./language");
var _deviceDescriptors = require("../deviceDescriptors");
var _javascript = require("./javascript");
var _utils = require("../../utils");
/**
 * Copyright (c) Microsoft Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

class JavaLanguageGenerator {
  constructor(mode) {
    this.id = void 0;
    this.groupName = 'Java';
    this.name = void 0;
    this.highlighter = 'java';
    this._mode = void 0;
    if (mode === 'library') {
      this.name = 'Library';
      this.id = 'java';
    } else if (mode === 'junit') {
      this.name = 'JUnit';
      this.id = 'java-junit';
    } else {
      throw new Error(`Unknown Java language mode: ${mode}`);
    }
    this._mode = mode;
  }
  generateAction(actionInContext) {
    const action = actionInContext.action;
    const pageAlias = actionInContext.frame.pageAlias;
    const offset = this._mode === 'junit' ? 4 : 6;
    const formatter = new _javascript.JavaScriptFormatter(offset);
    if (this._mode !== 'library' && (action.name === 'openPage' || action.name === 'closePage')) return '';
    if (action.name === 'openPage') {
      formatter.add(`Page ${pageAlias} = context.newPage();`);
      if (action.url && action.url !== 'about:blank' && action.url !== 'chrome://newtab/') formatter.add(`${pageAlias}.navigate(${quote(action.url)});`);
      return formatter.format();
    }
    const locators = actionInContext.frame.framePath.map(selector => `.${this._asLocator(selector, false)}.contentFrame()`);
    const subject = `${pageAlias}${locators.join('')}`;
    const signals = (0, _language.toSignalMap)(action);
    if (signals.dialog) {
      formatter.add(`  ${pageAlias}.onceDialog(dialog -> {
        System.out.println(String.format("Dialog message: %s", dialog.message()));
        dialog.dismiss();
      });`);
    }
    let code = this._generateActionCall(subject, actionInContext, !!actionInContext.frame.framePath.length);
    if (signals.popup) {
      code = `Page ${signals.popup.popupAlias} = ${pageAlias}.waitForPopup(() -> {
        ${code}
      });`;
    }
    if (signals.download) {
      code = `Download download${signals.download.downloadAlias} = ${pageAlias}.waitForDownload(() -> {
        ${code}
      });`;
    }
    formatter.add(code);
    return formatter.format();
  }
  _generateActionCall(subject, actionInContext, inFrameLocator) {
    const action = actionInContext.action;
    switch (action.name) {
      case 'openPage':
        throw Error('Not reached');
      case 'closePage':
        return `${subject}.close();`;
      case 'click':
        {
          let method = 'click';
          if (action.clickCount === 2) method = 'dblclick';
          const options = (0, _language.toClickOptionsForSourceCode)(action);
          const optionsText = formatClickOptions(options);
          return `${subject}.${this._asLocator(action.selector, inFrameLocator)}.${method}(${optionsText});`;
        }
      case 'check':
        return `${subject}.${this._asLocator(action.selector, inFrameLocator)}.check();`;
      case 'uncheck':
        return `${subject}.${this._asLocator(action.selector, inFrameLocator)}.uncheck();`;
      case 'fill':
        return `${subject}.${this._asLocator(action.selector, inFrameLocator)}.fill(${quote(action.text)});`;
      case 'setInputFiles':
        return `${subject}.${this._asLocator(action.selector, inFrameLocator)}.setInputFiles(${formatPath(action.files.length === 1 ? action.files[0] : action.files)});`;
      case 'press':
        {
          const modifiers = (0, _language.toKeyboardModifiers)(action.modifiers);
          const shortcut = [...modifiers, action.key].join('+');
          return `${subject}.${this._asLocator(action.selector, inFrameLocator)}.press(${quote(shortcut)});`;
        }
      case 'navigate':
        return `${subject}.navigate(${quote(action.url)});`;
      case 'select':
        return `${subject}.${this._asLocator(action.selector, inFrameLocator)}.selectOption(${formatSelectOption(action.options.length === 1 ? action.options[0] : action.options)});`;
      case 'assertText':
        return `assertThat(${subject}.${this._asLocator(action.selector, inFrameLocator)}).${action.substring ? 'containsText' : 'hasText'}(${quote(action.text)});`;
      case 'assertChecked':
        return `assertThat(${subject}.${this._asLocator(action.selector, inFrameLocator)})${action.checked ? '' : '.not()'}.isChecked();`;
      case 'assertVisible':
        return `assertThat(${subject}.${this._asLocator(action.selector, inFrameLocator)}).isVisible();`;
      case 'assertValue':
        {
          const assertion = action.value ? `hasValue(${quote(action.value)})` : `isEmpty()`;
          return `assertThat(${subject}.${this._asLocator(action.selector, inFrameLocator)}).${assertion};`;
        }
      case 'assertSnapshot':
        return `assertThat(${subject}.${this._asLocator(action.selector, inFrameLocator)}).matchesAriaSnapshot(${quote(action.snapshot)});`;
    }
  }
  _asLocator(selector, inFrameLocator) {
    return (0, _utils.asLocator)('java', selector, inFrameLocator);
  }
  generateHeader(options) {
    const formatter = new _javascript.JavaScriptFormatter();
    if (this._mode === 'junit') {
      formatter.add(`
      import com.microsoft.playwright.junit.UsePlaywright;
      import com.microsoft.playwright.Page;
      import com.microsoft.playwright.options.*;

      import org.junit.jupiter.api.*;
      import static com.microsoft.playwright.assertions.PlaywrightAssertions.*;

      @UsePlaywright
      public class TestExample {
        @Test
        void test(Page page) {`);
      return formatter.format();
    }
    formatter.add(`
    import com.microsoft.playwright.*;
    import com.microsoft.playwright.options.*;
    import static com.microsoft.playwright.assertions.PlaywrightAssertions.assertThat;
    import java.util.*;

    public class Example {
      public static void main(String[] args) {
        try (Playwright playwright = Playwright.create()) {
          Browser browser = playwright.${options.browserName}().launch(${formatLaunchOptions(options.launchOptions)});
          BrowserContext context = browser.newContext(${formatContextOptions(options.contextOptions, options.deviceName)});`);
    if (options.contextOptions.recordHar) formatter.add(`          context.routeFromHAR(${quote(options.contextOptions.recordHar.path)});`);
    return formatter.format();
  }
  generateFooter(saveStorage) {
    const storageStateLine = saveStorage ? `\n      context.storageState(new BrowserContext.StorageStateOptions().setPath(${quote(saveStorage)}));\n` : '';
    if (this._mode === 'junit') {
      return `${storageStateLine}  }
}`;
    }
    return `${storageStateLine}    }
  }
}`;
  }
}
exports.JavaLanguageGenerator = JavaLanguageGenerator;
function formatPath(files) {
  if (Array.isArray(files)) {
    if (files.length === 0) return 'new Path[0]';
    return `new Path[] {${files.map(s => 'Paths.get(' + quote(s) + ')').join(', ')}}`;
  }
  return `Paths.get(${quote(files)})`;
}
function formatSelectOption(options) {
  if (Array.isArray(options)) {
    if (options.length === 0) return 'new String[0]';
    return `new String[] {${options.map(s => quote(s)).join(', ')}}`;
  }
  return quote(options);
}
function formatLaunchOptions(options) {
  const lines = [];
  if (!Object.keys(options).filter(key => options[key] !== undefined).length) return '';
  lines.push('new BrowserType.LaunchOptions()');
  if (options.channel) lines.push(`  .setChannel(${quote(options.channel)})`);
  if (typeof options.headless === 'boolean') lines.push(`  .setHeadless(false)`);
  return lines.join('\n');
}
function formatContextOptions(contextOptions, deviceName) {
  const lines = [];
  if (!Object.keys(contextOptions).length && !deviceName) return '';
  const device = deviceName ? _deviceDescriptors.deviceDescriptors[deviceName] : {};
  const options = {
    ...device,
    ...contextOptions
  };
  lines.push('new Browser.NewContextOptions()');
  if (options.acceptDownloads) lines.push(`  .setAcceptDownloads(true)`);
  if (options.bypassCSP) lines.push(`  .setBypassCSP(true)`);
  if (options.colorScheme) lines.push(`  .setColorScheme(ColorScheme.${options.colorScheme.toUpperCase()})`);
  if (options.deviceScaleFactor) lines.push(`  .setDeviceScaleFactor(${options.deviceScaleFactor})`);
  if (options.geolocation) lines.push(`  .setGeolocation(${options.geolocation.latitude}, ${options.geolocation.longitude})`);
  if (options.hasTouch) lines.push(`  .setHasTouch(${options.hasTouch})`);
  if (options.isMobile) lines.push(`  .setIsMobile(${options.isMobile})`);
  if (options.locale) lines.push(`  .setLocale(${quote(options.locale)})`);
  if (options.proxy) lines.push(`  .setProxy(new Proxy(${quote(options.proxy.server)}))`);
  if (options.serviceWorkers) lines.push(`  .setServiceWorkers(ServiceWorkerPolicy.${options.serviceWorkers.toUpperCase()})`);
  if (options.storageState) lines.push(`  .setStorageStatePath(Paths.get(${quote(options.storageState)}))`);
  if (options.timezoneId) lines.push(`  .setTimezoneId(${quote(options.timezoneId)})`);
  if (options.userAgent) lines.push(`  .setUserAgent(${quote(options.userAgent)})`);
  if (options.viewport) lines.push(`  .setViewportSize(${options.viewport.width}, ${options.viewport.height})`);
  return lines.join('\n');
}
function formatClickOptions(options) {
  const lines = [];
  if (options.button) lines.push(`  .setButton(MouseButton.${options.button.toUpperCase()})`);
  if (options.modifiers) lines.push(`  .setModifiers(Arrays.asList(${options.modifiers.map(m => `KeyboardModifier.${m.toUpperCase()}`).join(', ')}))`);
  if (options.clickCount) lines.push(`  .setClickCount(${options.clickCount})`);
  if (options.position) lines.push(`  .setPosition(${options.position.x}, ${options.position.y})`);
  if (!lines.length) return '';
  lines.unshift(`new Locator.ClickOptions()`);
  return lines.join('\n');
}
function quote(text) {
  return (0, _utils.escapeWithQuotes)(text, '\"');
}