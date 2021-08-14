namespace AIBox {

  public class ChatBox : Sandbox.UI.ChatBox {

    public ChatBox() : base() {
      AddEventListener("onsubmit", () => PlayNotificationSound());
    }

    void PlayNotificationSound() {
      PlaySound("vj.playerspawn");
    }

  }

}