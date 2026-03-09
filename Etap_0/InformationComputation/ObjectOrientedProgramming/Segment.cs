//__________________________________________________________________________________________
//
//  Copyright 2022 Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and to get started
//  comment using the discussion panel at
//  https://github.com/mpostol/TP/discussions/182
//  with an introduction of yourself and tell us about what you do with this community.
//__________________________________________________________________________________________
#nullable disable annotations
namespace TP.InformationComputation.ObjectOrientedProgramming
{
  public class Segment : Coordinates
  {
    public Segment NextSegment { get; set; }

    public Segment(int x, int y, Segment next) : base(x, y)
    {
      NextSegment = next;
    }
  }

  public class Coordinates
  {
    public int x, y;

    public Coordinates(int p1, int p2)
    {
      x = p1;
      y = p2;
    }
  }
}
#nullable restore annotations
