# Stamina/Stamina Store Description

This is a sample of using [GS2-Stamina](https://app.gs2.io/docs/en/index.html#gs2-stamina) to manage stamina values.  
It is linked to [GS2-Exchange](https://app.gs2.io/docs/en/index.html#gs2-exchange) and uses the billing currency of [GS2-Money](https://app.gs2.io/docs/en/index.html#gs2-money) to manage stamina values. The following is a sample of a store function that consumes and recovers stamina values.

## GS2-Deploy template

- [initialize_stamina_template.yaml - Stamina/Stamina Store](../Templates/initialize_stamina_template.yaml)

## StaminaSetting StaminaSetting

![Inspector Window](Stamina.png)

| Setting Name | Description |
---|---
| staminaNamespaceName | Namespace name of GS2-Stamina
| staminaModelName | Stamina model name of GS2-Stamina
| staminaName | Name of the type of stamina in GS2-Stamina |
| exchangeNamespaceName | Namespace name of GS2-Exchange used to recover stamina
| exchangeRateName | Name of the GS2-Exchange exchange rate used to recover stamina
| exchangeKeyId | cryptographic key used to calculate the signature on the stamp sheet issued for the exchange process by GS2-Exchange
| distributorNamespaceName | namespace name of the GS2-Distributor delivering the exchanged stamina recovery process

| Event | Description |
---|---
| OnGetStamina(EzStamina stamina) | Called when stamina information is retrieved. | OnGetStamina(EzStamina stamina)
| OnBuy() | Called when an exchange is completed. | OnBuy()
| OnError(Gs2Exception error) | Called when an error occurs. | OnError(Gs2Exception error)

### Get Stamina

Get the latest stamina status.

```c#
AsyncResult<EzGetStaminaResult> result = null;
yield return client.Stamina.GetStamina(
    r =>
    {
        result = r;
    },
    session,
    staminaNamespaceName,
    staminaName
);
```

### Consume Stamina

Consumes 5 stamina.

```c#
AsyncResult<EzConsumeResult> result = null;
yield return client.Stamina.Consume(
    r =>
    {
        result = r;
    },
    session,
    staminaNamespaceName,
    Name,
    consumeValue
);
```

### Purchase of Stamina Restoration

Executes the process of purchasing stamina recovery.

```c#
AsyncResult<EzExchangeResult> result = null;
yield return gs2Client.client.Exchange.Exchange(
    r => { result = r; }
    request.gameSession,
    gs2StaminaSetting.exchangeNamespaceName,
    gs2StaminaSetting.exchangeRateName,
    1,
    new List<Gs2.Unity.Gs2Exchange.Model.EzConfig>
    {
        New Gs2.Unity.Gs2Exchange.Model.EzConfig
        {
            Key = "slot",
            Value = MoneyController.Slot.ToString(),
        }
    }
);
```

Since the stamp sheet is returned in `result.Result.StampSheet`.

```c#
var machine = new StampSheetStateMachine(
    stampSheet,
    gs2Client.client,
    gs2StaminaSetting.distributorNamespaceName,
    gs2StaminaSetting.exchangeKeyId
);
yield return machine.Execute();
```

By executing the stamp sheet in this manner, the actual exchange of charged currency for stamina values is performed.

![Exchange](Exchange.png)