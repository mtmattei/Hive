using Hive.Core.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Hive.Controls;

public sealed partial class RewardCard : UserControl
{
    public static readonly DependencyProperty RewardProperty =
        DependencyProperty.Register(nameof(Reward), typeof(Reward), typeof(RewardCard),
            new PropertyMetadata(null, OnDataChanged));

    public static readonly DependencyProperty StarBalanceProperty =
        DependencyProperty.Register(nameof(StarBalance), typeof(int), typeof(RewardCard),
            new PropertyMetadata(0, OnDataChanged));

    public Reward? Reward
    {
        get => (Reward?)GetValue(RewardProperty);
        set => SetValue(RewardProperty, value);
    }

    public int StarBalance
    {
        get => (int)GetValue(StarBalanceProperty);
        set => SetValue(StarBalanceProperty, value);
    }

    public event EventHandler<Reward>? RedeemClicked;
    public event EventHandler<Reward>? EditClicked;

    public RewardCard()
    {
        InitializeComponent();
        RedeemButton.Click += (_, _) =>
        {
            if (Reward is not null)
                RedeemClicked?.Invoke(this, Reward);
        };
    }

    private static void OnDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is RewardCard card) card.UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (Reward is null) return;

        EmojiText.Text = Reward.Emoji ?? "🏆";
        TitleText.Text = Reward.Title;

        var progress = Reward.StarCost > 0
            ? Math.Min(1.0, (double)StarBalance / Reward.StarCost)
            : 0;

        ProgressFill.Width = progress * 200;
        ProgressText.Text = $"{StarBalance} / {Reward.StarCost}";
        RedeemButton.IsEnabled = StarBalance >= Reward.StarCost;
    }
}
